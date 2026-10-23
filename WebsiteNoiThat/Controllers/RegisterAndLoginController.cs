using Models.DAO;
using Models.EF;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Areas.Admin.Models;
using WebsiteNoiThat.Common;
using WebsiteNoiThat.Models;

namespace WebsiteNoiThat.Controllers
{
    public class RegisterAndLoginController : Controller
    {
        private DBNoiThat db = new DBNoiThat();

        #region Helper: Kiểm tra xác minh "Tôi không phải là robot"
        /// <summary>
        /// Hàm kiểm tra Captcha: Hỗ trợ xác thực Google reCAPTCHA v2 và checkbox dự phòng
        /// </summary>
        private bool ValidateRobotCaptcha()
        {
            string googleCaptchaToken = Request["g-recaptcha-response"];
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            // 1. Nếu có token từ Google reCAPTCHA
            if (!string.IsNullOrEmpty(googleCaptchaToken))
            {
                try
                {
                    string secretKey = ConfigurationManager.AppSettings["GoogleReCaptchaSecretKey"] 
                                       ?? "6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe"; // Key test mặc định của Google
                    
                    using (var client = new WebClient())
                    {
                        string verifyUrl = string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secretKey, googleCaptchaToken);
                        string response = client.DownloadString(verifyUrl);
                        var json = JObject.Parse(response);
                        return (bool)json["success"];
                    }
                }
                catch
                {
                    return false;
                }
            }

            // 2. Dự phòng: Nếu view đang dùng checkbox html thông thường (name="IsNotRobot")
            if (Request.Form["IsNotRobot"] == "true")
            {
                return true;
            }

            return false;
        }
        #endregion

        // ==================== 1. ĐĂNG XUẤT ====================
        public ActionResult Logout()
        {
            Session[Commoncontent.user_sesion] = null;
            Session[Commoncontent.CartSession] = null;
            return Redirect("/");
        }

        // ==================== 2. ĐĂNG NHẬP ====================
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Models.LoginModel model)
        {
            // Kiểm tra "Tôi không phải là robot"
            if (!ValidateRobotCaptcha())
            {
                ModelState.AddModelError("", "Vui lòng xác nhận bạn không phải là robot!");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var dao = new UserDao();
                var result = dao.Login(model.UserName, Encryptor.MD5Hash(model.Password));

                if (result == 1)
                {
                    var user = dao.GetUserForLogin(model.UserName);
                    var userSession = new UserLogin();
                    userSession.Username = user.Username;
                    userSession.UserId = user.UserId;
                    Session.Add(Commoncontent.user_sesion, userSession);
                    return Redirect("/");
                }
                else if (result == -1)
                {
                    ModelState.AddModelError("", "Tài khoản đang bị khóa, liên hệ admin.");
                }
                else if (result == 0)
                {
                    ModelState.AddModelError("", "Tài khoản không tồn tại.");
                }
                else if (result == -2)
                {
                    ModelState.AddModelError("", "Mật khẩu không đúng.");
                }
            }
            return View(model);
        }

        // ==================== 3. ĐĂNG KÝ ====================
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            // 1. Kiểm tra Captcha
            if (!ValidateRobotCaptcha())
            {
                ModelState.AddModelError("", "Vui lòng xác nhận bạn không phải là robot!");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var dao = new UserDao();
                if (dao.CheckUserName(model.UserName))
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                // Kiểm tra email đã tồn tại hay chưa
                if (db.Users.Any(x => x.Email.ToLower() == model.Email.Trim().ToLower()))
                {
                    ModelState.AddModelError("", "Địa chỉ email này đã được sử dụng.");
                    return View(model);
                }

                // 2. Tạo mã OTP gửi về Email xác nhận
                string otp = MailHelper.GenerateOTP();
                Session["Register_Data"] = model;
                Session["Register_OTP"] = otp;
                Session["Register_ExpireTime"] = DateTime.Now.AddMinutes(5);

                string body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e3e6f0; border-radius: 8px; max-width: 500px;'>
                        <h3 style='color: #28a745; margin-top: 0;'>Xác thực đăng ký tài khoản</h3>
                        <p>Xin chào <b>{model.Name}</b>,</p>
                        <p>Cảm ơn bạn đã đăng ký tài khoản. Mã OTP xác thực đăng ký của bạn là:</p>
                        <div style='text-align: center; margin: 20px 0;'>
                            <span style='font-size: 28px; font-weight: bold; color: #28a745; letter-spacing: 6px; background: #f8f9fc; padding: 10px 20px; border-radius: 6px; border: 1px dashed #d1d3e2;'>{otp}</span>
                        </div>
                        <p style='color: #858796; font-size: 13px;'>Mã này có hiệu lực trong <b>5 phút</b>. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                    </div>";
            try{
                bool sent = MailHelper.SendMail(model.Email, "Mã OTP kích hoạt tài khoản - Website Nội Thất", body);
                if (!sent)
                {
                    ModelState.AddModelError("", "Không thể gửi email xác thực từ máy chủ SMTP. Vui lòng kiểm tra lại cấu hình Web.config!");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi ngoại lệ khi gửi mail: " + ex.Message);
                return View(model);
            }

                // Chuyển sang trang nhập OTP xác thực
                return RedirectToAction("VerifyRegisterOtp");
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult VerifyRegisterOtp()
        {
            var registerData = Session["Register_Data"] as RegisterModel;
            if (registerData == null)
            {
                return RedirectToAction("Register");
            }
            ViewBag.Email = registerData.Email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyRegisterOtp(string otpInput)
        {
            var registerData = Session["Register_Data"] as RegisterModel;
            string sessionOtp = Session["Register_OTP"] as string;
            DateTime? expireTime = Session["Register_ExpireTime"] as DateTime?;

            if (registerData == null || expireTime == null)
            {
                return RedirectToAction("Register");
            }

            ViewBag.Email = registerData.Email;

            // Kiểm tra hết hạn
            if (DateTime.Now > expireTime)
            {
                ModelState.AddModelError("", "Mã OTP đã hết hạn! Vui lòng nhấn 'Gửi lại mã OTP' để nhận mã mới.");
                return View();
            }

            // Kiểm tra mã OTP
            if (string.IsNullOrWhiteSpace(otpInput) || sessionOtp != otpInput.Trim())
            {
                ModelState.AddModelError("", "Mã OTP không chính xác!");
                return View();
            }

            // OTP đúng -> Bắt đầu ghi người dùng vào CSDL
            int.TryParse(registerData.Phone, out int phone); // Regex đã đảm bảo chuỗi là số
            var dao = new UserDao();
            var user = new User
            {
                Username = registerData.UserName,
                Password = Encryptor.MD5Hash(registerData.Password),
                Phone = phone,
                Email = registerData.Email,
                Address = registerData.Address,
                Name = registerData.Name,
                GroupId = "USER",
                Status = true // Kích hoạt sau khi xác thực email thành công
            };

            var result = dao.Insert(user);
            if (result > 0)
            {
                // Dọn dẹp session
                Session.Remove("Register_Data");
                Session.Remove("Register_OTP");
                Session.Remove("Register_ExpireTime");

                var newUser = db.Users.SingleOrDefault(n => n.Username == registerData.UserName);
                return RedirectToAction("Card", new { UserId = newUser.UserId });
            }

            ModelState.AddModelError("", "Đăng ký không thành công. Vui lòng thử lại!");
            return View();
        }

        // Gửi lại mã OTP đăng ký (khi hết hạn 5 phút hoặc khách chưa nhận được thư)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResendRegisterOtp()
        {
            var registerData = Session["Register_Data"] as RegisterModel;
            if (registerData == null)
            {
                return RedirectToAction("Register");
            }

            string otp = MailHelper.GenerateOTP();
            Session["Register_OTP"] = otp;
            Session["Register_ExpireTime"] = DateTime.Now.AddMinutes(5);

            string body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e3e6f0; border-radius: 8px; max-width: 500px;'>
                    <h3 style='color: #28a745; margin-top: 0;'>Xác thực đăng ký tài khoản</h3>
                    <p>Xin chào <b>{registerData.Name}</b>,</p>
                    <p>Mã OTP xác thực đăng ký (mới) của bạn là:</p>
                    <div style='text-align: center; margin: 20px 0;'>
                        <span style='font-size: 28px; font-weight: bold; color: #28a745; letter-spacing: 6px; background: #f8f9fc; padding: 10px 20px; border-radius: 6px; border: 1px dashed #d1d3e2;'>{otp}</span>
                    </div>
                    <p style='color: #858796; font-size: 13px;'>Mã này có hiệu lực trong <b>5 phút</b>. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                </div>";

            bool sent = MailHelper.SendMail(registerData.Email, "Mã OTP kích hoạt tài khoản (gửi lại) - Website Nội Thất", body);
            if (!sent)
            {
                TempData["OtpError"] = "Không thể gửi lại email xác thực. Vui lòng thử lại sau!";
                return RedirectToAction("VerifyRegisterOtp");
            }

            TempData["OtpResent"] = "Đã gửi lại mã OTP mới về email của bạn.";
            return RedirectToAction("VerifyRegisterOtp");
        }
        // ==================== 4. QUÊN MẬT KHẨU ====================
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string Email)
        {
            // 1. BẮT BUỘC KIỂM TRA CAPTCHA TRƯỚC TIÊN
            if (!ValidateRobotCaptcha())
            {
                ModelState.AddModelError("", "Vui lòng xác minh 'Tôi không phải là người máy'!");
                return View();
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ModelState.AddModelError("", "Vui lòng nhập địa chỉ email!");
                return View();
            }

            // 2. Kiểm tra email có trong Database không
            var user = db.Users.FirstOrDefault(x => x.Email.ToLower() == Email.Trim().ToLower());
            if (user == null)
            {
                ModelState.AddModelError("", "Email này chưa được đăng ký trong hệ thống!");
                return View();
            }

            // 3. Tạo mã OTP (dùng MailHelper chuẩn)
            string otp = MailHelper.GenerateOTP();
            Session["Reset_OTP"] = otp;
            Session["Reset_Email"] = user.Email;
            Session["Reset_ExpireTime"] = DateTime.Now.AddMinutes(5);

            // 4. Nội dung gửi mail
            string body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e3e6f0; border-radius: 8px; max-width: 500px;'>
                    <h3 style='color: #4e73df; margin-top: 0;'>Khôi phục mật khẩu</h3>
                    <p>Xin chào <b>{user.Name ?? user.Username}</b>,</p>
                    <p>Mã OTP xác thực đặt lại mật khẩu của bạn là:</p>
                    <div style='text-align: center; margin: 20px 0;'>
                        <span style='font-size: 28px; font-weight: bold; color: #e74a3b; letter-spacing: 6px; background: #f8f9fc; padding: 10px 20px; border-radius: 6px; border: 1px dashed #d1d3e2;'>{otp}</span>
                    </div>
                    <p style='color: #858796; font-size: 13px;'>Mã này có hiệu lực trong <b>5 phút</b>. Vui lòng không cung cấp mã này cho người khác.</p>
                </div>";

            bool sent = MailHelper.SendMail(user.Email, "Mã OTP khôi phục mật khẩu - Website Nội Thất", body);
            if (!sent)
            {
                ModelState.AddModelError("", "Không thể gửi email. Vui lòng kiểm tra lại cấu hình hoặc thử lại sau!");
                return View();
            }

            return RedirectToAction("ConfirmResetOtp");
        }

        [HttpGet]
        public ActionResult ConfirmResetOtp()
        {
            if (Session["Reset_Email"] == null) 
            {
                return RedirectToAction("ForgotPassword");
            }
            ViewBag.Email = Session["Reset_Email"].ToString();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmResetOtp(string otpInput, string newPassword, string confirmPassword)
        {
            string sessionOtp = Session["Reset_OTP"] as string;
            string email = Session["Reset_Email"] as string;
            DateTime? expireTime = Session["Reset_ExpireTime"] as DateTime?;

            ViewBag.Email = email;

            // Kiểm tra hết hạn Session / OTP
            if (expireTime == null || DateTime.Now > expireTime)
            {
                ModelState.AddModelError("", "Mã OTP đã hết hạn! Vui lòng yêu cầu lại mã mới.");
                return View();
            }

            // Kiểm tra đúng mã OTP
            if (string.IsNullOrWhiteSpace(otpInput) || sessionOtp != otpInput.Trim())
            {
                ModelState.AddModelError("", "Mã OTP không chính xác!");
                return View();
            }

            // Kiểm tra độ dài mật khẩu mới
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu mới phải từ 6 ký tự trở lên!");
                return View();
            }

            // Kiểm tra khớp mật khẩu
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp!");
                return View();
            }

            var user = db.Users.FirstOrDefault(x => x.Email == email);
            if (user != null)
            {
                // FIX BUG: PHẢI DÙNG MD5 HASH ĐỂ ĐỒNG BỘ VỚI HÀM LOGIN
                user.Password = Encryptor.MD5Hash(newPassword);
                db.SaveChanges();

                // Xóa sạch Session sau khi hoàn thành
                Session.Remove("Reset_OTP");
                Session.Remove("Reset_Email");
                Session.Remove("Reset_ExpireTime");

                TempData["Success"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập bằng mật khẩu mới.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Không tìm thấy tài khoản để cập nhật!");
            return View();
        }
        // ==================== 5. THÔNG TIN NGƯỜI DÙNG ====================
        [HttpGet]
        public ActionResult ViewCurentUser()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
            if (session != null)
            {
                var model = db.Users.SingleOrDefault(n => n.UserId == session.UserId);
                return View(model);
            }
            else
            {
                return Redirect("/RegisterAndLogin/Login");
            }
        }

        [HttpGet]
        public ActionResult EditCurentUser()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
            if (session == null)
            {
                return RedirectToAction("Login", "RegisterAndLogin");
            }

            var model = db.Users.SingleOrDefault(n => n.UserId == session.UserId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCurentUser([Bind(Include = "UserId,Name,Address,Phone,Username,Email,GroupId,Status")] User user)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
            if (session == null)
            {
                return RedirectToAction("Login", "RegisterAndLogin");
            }

            if (ModelState.IsValid)
            {
                var existingUser = db.Users.AsNoTracking().FirstOrDefault(n => n.UserId == user.UserId && n.UserId == session.UserId);
                if (existingUser == null)
                {
                    return RedirectToAction("ViewCurentUser");
                }

                user.Password = existingUser.Password;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ViewCurentUser");
            }
            return View(user);
        }

        // ==================== 6. ĐỔI MẬT KHẨU ====================
        [HttpGet]
        public ActionResult ChangePassword()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
            if (session == null)
            {
                return RedirectToAction("Login", "RegisterAndLogin");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
            if (session == null)
            {
                return RedirectToAction("Login", "RegisterAndLogin");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = db.Users.Find(session.UserId);
            if (user == null)
            {
                return RedirectToAction("Login", "RegisterAndLogin");
            }

            var oldPasswordHash = Encryptor.MD5Hash(model.OldPassword);
            if (!string.Equals(user.Password?.Trim(), oldPasswordHash, StringComparison.Ordinal) &&
                !string.Equals(user.Password?.Trim(), model.OldPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError("", "Mật khẩu cũ không chính xác.");
                return View(model);
            }

            if (string.Equals(model.OldPassword, model.NewPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError("", "Mật khẩu mới không được trùng với mật khẩu cũ.");
                return View(model);
            }

            user.Password = Encryptor.MD5Hash(model.NewPassword);
            db.SaveChanges();

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index", "Home");
        }

        // ==================== 7. THẺ TÍCH ĐIỂM ====================
        [HttpGet]
        public ActionResult Card(int UserId)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
            if (session != null)
            {
                var checkuser = db.Cards.SingleOrDefault(n => n.UserId == session.UserId);
                if (checkuser == null)
                {
                    var m = db.Users.SingleOrDefault(n => n.UserId == UserId);
                    if (m != null)
                    {
                        var model = new Card();
                        model.UserId = session.UserId;
                        model.NumberCard = 0;
                        model.UserNumber = 0;
                        return View(model);
                    }
                    else
                    {
                        var model = new Card();
                        model.UserId = session.UserId;
                        model.NumberCard = 0;
                        model.UserNumber = 0;
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Đã có thẻ tích điểm. Bạn không thể đăng ký thêm.");
                    return View();
                }
            }
            else
            {
                var model = new Card();
                model.UserId = UserId;
                model.NumberCard = 0;
                model.UserNumber = 0;
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult Card(Card n)
        {
            var model = new Card();
            model.UserId = n.UserId;
            model.NumberCard = 0;
            model.UserNumber = 0;
            model.Identification = n.Identification;

            db.Cards.Add(model);
            db.SaveChanges();
            ViewBag.Success = "Đăng ký thẻ thành công";
            return Redirect("/");
        }

        // ==================== 8. XEM THÔNG TIN ĐĂNG NHẬP (HEADER/MENU) ====================
        public ActionResult ViewLogin()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
            if (session != null)
            {
                var model = db.Cards.SingleOrDefault(n => n.UserId == session.UserId);
                var models = (from a in db.OrderDetails
                              join b in db.Orders on a.OrderId equals b.OrderId
                              join c in db.Products on a.ProductId equals c.ProductId
                              join d in db.Users on b.UserId equals d.UserId
                              join e in db.Cards on d.UserId equals e.UserId
                              where b.StatusId == 5 && e.UserId == session.UserId
                              select new
                              {
                                  ProductId = a.ProductId,
                                  Price = a.Price,
                                  Quantity = a.Quantity,
                                  Discount = c.Discount,
                                  NumberCard = e.NumberCard,
                                  Username = d.Username
                              }).ToList();

                if (models.Count() == 0)
                {
                    ViewBag.Card = 0;
                }
                else
                {
                    double? total = 0;
                    foreach (var item in models)
                    {
                        total += ((item.Price.GetValueOrDefault(0) - (item.Price.GetValueOrDefault(0) * item.Discount.GetValueOrDefault(0) * 0.01)) * item.Quantity);
                    }

                    if (model != null)
                    {
                        model.NumberCard = Convert.ToInt32(total / 1000) - model.UserNumber;
                        db.SaveChanges();
                        ViewBag.Card = model.NumberCard;
                    }
                }
            }
            return PartialView();
        }
    }
}
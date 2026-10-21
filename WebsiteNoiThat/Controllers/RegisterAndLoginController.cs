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
                    var user = dao.GetById(model.UserName);
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
            // Kiểm tra "Tôi không phải là robot" thay thế BotDetect cũ
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
                }
                else
                {
                    var user = new User();
                    user.Username = model.UserName;
                    user.Password = Encryptor.MD5Hash(model.Password);
                    user.Phone = model.Phone;
                    user.Email = model.Email;
                    user.Address = model.Address;
                    user.Name = model.Name;
                    user.GroupId = "USER";
                    user.Status = true;

                    var result = dao.Insert(user);
                    if (result > 0)
                    {
                        ViewBag.Success = "Đăng ký thành công";
                        var models = db.Users.SingleOrDefault(n => n.Username == model.UserName);
                        return RedirectToAction("Card", new { UserId = models.UserId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Đăng ký không thành công.");
                    }
                }
            }
            return View(model);
        }

        // ==================== 4. QUÊN MẬT KHẨU ====================
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                email = Request.Form["Email"];
            }

            if (!ValidateRobotCaptcha())
            {
                ModelState.AddModelError("", "Vui lòng xác nhận bạn không phải là robot!");
                ViewBag.Email = email;
                return View();
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Vui lòng nhập địa chỉ Email đăng ký!");
                return View();
            }

            var user = db.Users.FirstOrDefault(x => x.Email == email.Trim());
            if (user == null)
            {
                ModelState.AddModelError("", "Địa chỉ Email này không tồn tại trong hệ thống.");
                ViewBag.Email = email;
                return View();
            }

            // Tạo mật khẩu ngẫu nhiên mới
            string newPassword = new Random().Next(100000, 999999).ToString();
            user.Password = Encryptor.MD5Hash(newPassword);
            db.SaveChanges();

            // Hiển thị trực tiếp mật khẩu mới lên màn hình (không cần gửi email)
            ViewBag.Success = "Khôi phục thành công! Mật khẩu mới của tài khoản (" + user.Username + ") là: " + newPassword;

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
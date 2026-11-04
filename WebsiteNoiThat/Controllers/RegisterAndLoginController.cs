using Models.DAO;
using Models.EF;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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

            // Nếu là Admin đăng nhập từ trang khách -> xóa luôn phiên quản trị để không còn quyền truy cập /Admin sau khi đăng xuất
            Session[Commoncontent.user_sesion_admin] = null;
            Session[Commoncontent.SESSION_CREDENTIALS] = null;

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

                    // Tài khoản Admin đăng nhập từ trang khách -> vào thẳng khu quản lý,
                    // khách hàng thường -> về trang chủ mua hàng (issue #95)
                    if (user.GroupId == CommonConstant.ADMIN_GROUP)
                    {
                        var adminSession = new UserLogin();
                        adminSession.Username = user.Username;
                        adminSession.UserId = user.UserId;
                        adminSession.GroupId = user.GroupId;

                        var listCredentials = dao.GetListCredentials(user.Username);
                        Session.Add(Commoncontent.SESSION_CREDENTIALS, listCredentials);
                        Session.Add(Commoncontent.user_sesion_admin, adminSession);

                        return Redirect("/Admin");
                    }

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
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model, HttpPostedFileBase frontDocument,
            HttpPostedFileBase backDocument, bool documentConsent = false)
        {
            // 1. Kiểm tra Captcha
            if (!ValidateRobotCaptcha())
            {
                ModelState.AddModelError("", "Vui lòng xác nhận bạn không phải là robot!");
                return View(model);
            }

            // 2. Kiem tra do manh mat khau (issue #130)
            if (!PasswordPolicy.IsValid(model.Password))
            {
                ModelState.AddModelError("Password", PasswordPolicy.RuleMessage);
                return View(model);
            }

            if (model.DocumentType != "CCCD" && model.DocumentType != "Passport")
            {
                ModelState.AddModelError("DocumentType", "Vui lòng chọn CCCD hoặc hộ chiếu.");
            }
            if (!documentConsent)
            {
                ModelState.AddModelError("", "Bạn cần đồng ý với việc sử dụng ảnh giấy tờ để xác minh danh tính.");
            }
            if (model.DocumentType == "CCCD" && (backDocument == null || backDocument.ContentLength <= 0))
            {
                ModelState.AddModelError("backDocument", "CCCD cần ảnh cả mặt trước và mặt sau.");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    IdentityDocumentStorage.ComputeDocumentNumberHash(model.DocumentNumber);
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("DocumentNumber", ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    System.Diagnostics.Trace.TraceError("Identity document HMAC configuration error: " + ex);
                    ModelState.AddModelError("", "Chức năng xác minh giấy tờ chưa được cấu hình an toàn. Vui lòng liên hệ quản trị viên.");
                }
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

                string frontFileName = null;
                string backFileName = null;
                try
                {
                    frontFileName = IdentityDocumentStorage.Save(frontDocument);
                    if (model.DocumentType == "CCCD")
                    {
                        backFileName = IdentityDocumentStorage.Save(backDocument);
                    }
                }
                catch (InvalidDataException ex)
                {
                    IdentityDocumentStorage.Delete(frontFileName);
                    IdentityDocumentStorage.Delete(backFileName);
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }

                string oldFrontFileName = Session["Register_IdentityFrontFile"] as string;
                string oldBackFileName = Session["Register_IdentityBackFile"] as string;
                Session["Register_IdentityFrontFile"] = frontFileName;
                Session["Register_IdentityBackFile"] = backFileName;
                IdentityDocumentStorage.Delete(oldFrontFileName);
                IdentityDocumentStorage.Delete(oldBackFileName);

                // 2. Tạo mã OTP gửi về Email xác nhận
                string otp = MailHelper.GenerateOTP();
                DateTime otpExpireTime = DateTime.Now.AddMinutes(5);
                Session["Register_Data"] = model;
                Session["Register_OTP"] = otp;
                Session["Register_ExpireTime"] = otpExpireTime;

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

            // Chế độ dev: mail phải lưu ra file (SMTP không gửi được) -> hiện luôn mã OTP
            // trên trang xác thực để tester không phải đi mở file (dựa theo PR #85)
            if (MailHelper.LastSentViaDevFallback)
            {
                TempData["DevOtp"] = otp;
            }
            StoreDemoOtpOnHome(otp, otpExpireTime);

            // Issue #114: đồng thời gửi OTP vào kênh Chat nội bộ của site
            SendOtpToChat(model.Email, ChatMessageDao.NormalizePhone(model.Phone),
                string.Format("Mã OTP kích hoạt tài khoản của bạn là: {0}\nMã có hiệu lực trong 5 phút. Vui lòng không chia sẻ cho bất kỳ ai.", otp),
                model.Name ?? model.UserName);
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

            var frontFileName = Session["Register_IdentityFrontFile"] as string;
            var backFileName = Session["Register_IdentityBackFile"] as string;
            if (string.IsNullOrWhiteSpace(frontFileName))
            {
                ModelState.AddModelError("", "Không tìm thấy ảnh giấy tờ đã gửi. Vui lòng đăng ký lại.");
                return View();
            }

            string documentNumberHash;
            try
            {
                documentNumberHash = IdentityDocumentStorage.ComputeDocumentNumberHash(registerData.DocumentNumber);
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Trace.TraceError("Identity document HMAC configuration error: " + ex);
                ModelState.AddModelError("", "Chức năng xác minh giấy tờ chưa được cấu hình an toàn. Vui lòng liên hệ quản trị viên.");
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
                var newUser = db.Users.SingleOrDefault(n => n.Username == registerData.UserName);
                if (newUser == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy tài khoản vừa tạo. Vui lòng liên hệ hỗ trợ.");
                    return View();
                }

                db.IdentityVerifications.Add(new IdentityVerification
                {
                    UserId = newUser.UserId,
                    DocumentType = registerData.DocumentType,
                    DocumentNumberHash = documentNumberHash,
                    FrontFileName = frontFileName,
                    BackFileName = backFileName,
                    Status = "Pending",
                    SubmittedAt = DateTime.Now
                });
                db.SaveChanges();

                Session["IdentityVerificationUserId"] = newUser.UserId;
                Session.Remove("Register_Data");
                Session.Remove("Register_OTP");
                Session.Remove("Register_ExpireTime");
                Session.Remove("DemoOtp");
                Session.Remove("DemoOtpExpireTime");
                Session.Remove("Register_IdentityFrontFile");
                Session.Remove("Register_IdentityBackFile");

                // Issue #114: sau khi tài khoản được tạo, gắn thread chat "khách" (trước đó
                // đã nhận tin nhắn OTP đăng ký) vào UserId mới để khách login xem đủ lịch sử
                try
                {
                    using (var chatDao = new ChatMessageDao())
                    {
                        chatDao.LinkGuestThreadToUser(registerData.Email,
                            ChatMessageDao.NormalizePhone(registerData.Phone), newUser.UserId);
                    }
                }
                catch (SqlException ex) when (ex.Number == 208)
                {
                    System.Diagnostics.Trace.TraceError("ChatMessage table is missing: " + ex);
                }

                return RedirectToAction("Submit", "IdentityVerification");
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
            DateTime otpExpireTime = DateTime.Now.AddMinutes(5);
            Session["Register_OTP"] = otp;
            Session["Register_ExpireTime"] = otpExpireTime;

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
            // Chế độ dev: mail gửi qua kênh dự phòng -> hiện mã OTP mới ngay trên trang xác thực
            if (MailHelper.LastSentViaDevFallback)
            {
                TempData["DevOtp"] = otp;
            }
            StoreDemoOtpOnHome(otp, otpExpireTime);

            TempData["OtpResent"] = "Đã gửi lại mã OTP mới về email của bạn.";

            // Issue #114: gửi OTP vào chat nội bộ (thread đang chờ xác thực)
            SendOtpToChat(registerData.Email, ChatMessageDao.NormalizePhone(registerData.Phone),
                string.Format("Mã OTP kích hoạt tài khoản của bạn là: {0}\nMã có hiệu lực trong 5 phút.", otp),
                registerData.Name ?? registerData.UserName);

            return RedirectToAction("VerifyRegisterOtp");
        }
        // ==================== 4. QUÊN MẬT KHẨU ====================
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            int? verificationId = Session["ResetVerificationId"] as int?;
            if (verificationId.HasValue)
            {
                var verification = db.PasswordResetVerifications.Find(verificationId.Value);
                if (verification != null)
                {
                    ViewBag.ResetVerificationStatus = verification.Status;
                    ViewBag.ResetVerificationNote = verification.ReviewNote;
                    if (verification.Status == "Approved")
                    {
                        PopulateResetOtpView(verification);
                    }
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string Email, string DocumentType,
            string DocumentNumber, HttpPostedFileBase frontDocument, HttpPostedFileBase backDocument,
            bool documentConsent = false)
        {
            ViewBag.Email = Email;
            ViewBag.DocumentType = DocumentType;
            ViewBag.DocumentNumber = DocumentNumber;
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
            var user = db.Users.FirstOrDefault(x => x.Email.ToLower() == Email.Trim().ToLower());
            if (user == null)
            {
                ModelState.AddModelError("", "Email này chưa được đăng ký trong hệ thống!");
                return View();
            }

            IdentityVerification identityVerification;
            try
            {
                identityVerification = db.IdentityVerifications
                    .SingleOrDefault(x => x.UserId == user.UserId);
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                System.Diagnostics.Trace.TraceError("IdentityVerification table is missing: " + ex);
                ModelState.AddModelError("", "Chức năng xác minh chưa được cài đặt trong cơ sở dữ liệu. " +
                    "Hãy chạy SQL/2026-11-03_tao_bang_identityverification.sql trên DBNoiThat.");
                return View();
            }
            if (identityVerification == null || identityVerification.Status != "Approved")
            {
                ModelState.AddModelError("", "Tài khoản chưa được ADMIN/MOD xác minh giấy tờ tùy thân. " +
                    "Hãy hoàn tất hồ sơ xác minh trước khi yêu cầu đặt lại mật khẩu, hoặc liên hệ hỗ trợ nếu hồ sơ bị từ chối.");
                return View();
            }

            if (string.IsNullOrWhiteSpace(identityVerification.DocumentNumberHash) ||
                string.IsNullOrWhiteSpace(identityVerification.DocumentType))
            {
                ModelState.AddModelError("", "Hồ sơ đăng ký chưa có dữ liệu giấy tờ để đối chiếu. Vui lòng liên hệ quản trị viên.");
                return View();
            }
            if (string.IsNullOrWhiteSpace(identityVerification.FrontFileName))
            {
                ModelState.AddModelError("", "Hồ sơ đăng ký không còn ảnh giấy tờ gốc để so sánh. Vui lòng liên hệ quản trị viên.");
                return View();
            }
            if (DocumentType != identityVerification.DocumentType)
            {
                ModelState.AddModelError("DocumentType", "Loại giấy tờ không trùng với hồ sơ đã xác minh.");
                return View();
            }
            if (string.IsNullOrWhiteSpace(DocumentNumber))
            {
                ModelState.AddModelError("DocumentNumber", "Vui lòng nhập số giấy tờ đã đăng ký.");
                return View();
            }
            bool documentNumberMatches;
            try
            {
                documentNumberMatches = IdentityDocumentStorage.VerifyDocumentNumber(
                    DocumentNumber, identityVerification.DocumentNumberHash);
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Trace.TraceError("Identity document HMAC configuration error: " + ex);
                ModelState.AddModelError("", "Không thể kiểm tra số giấy tờ do cấu hình bảo mật chưa hợp lệ.");
                return View();
            }
            if (!documentNumberMatches)
            {
                ModelState.AddModelError("DocumentNumber", "Số giấy tờ không khớp với hồ sơ đăng ký đã được duyệt.");
                return View();
            }
            if (!documentConsent)
            {
                ModelState.AddModelError("", "Bạn cần đồng ý cho ADMIN/MOD đối chiếu ảnh tải lên với ảnh giấy tờ gốc.");
                return View();
            }
            if (frontDocument == null || frontDocument.ContentLength <= 0)
            {
                ModelState.AddModelError("", "Vui lòng tải ảnh mặt trước hoặc trang thông tin hộ chiếu lên.");
                return View();
            }
            if (DocumentType == "CCCD" && (backDocument == null || backDocument.ContentLength <= 0))
            {
                ModelState.AddModelError("", "CCCD cần ảnh cả mặt trước và mặt sau.");
                return View();
            }

            PasswordResetVerification existing;
            try
            {
                existing = db.PasswordResetVerifications
                    .SingleOrDefault(x => x.UserId == user.UserId);
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                System.Diagnostics.Trace.TraceError("PasswordResetVerification table is missing: " + ex);
                ModelState.AddModelError("", "Chức năng khôi phục chưa được cài đặt trong cơ sở dữ liệu. " +
                    "Hãy chạy SQL/2026-11-03_tao_bang_passwordresetverification.sql trên DBNoiThat.");
                return View();
            }

            if (existing != null && existing.Status == "Pending" &&
                !string.IsNullOrWhiteSpace(existing.FrontFileName))
            {
                Session["ResetVerificationId"] = existing.PasswordResetVerificationId;
                Session["Reset_Email"] = user.Email;
                return RedirectToAction("ForgotPassword");
            }

            string oldFrontFileName = existing == null ? null : existing.FrontFileName;
            string oldBackFileName = existing == null ? null : existing.BackFileName;
            string newFrontFileName = null;
            string newBackFileName = null;
            try
            {
                newFrontFileName = IdentityDocumentStorage.Save(frontDocument);
                if (DocumentType == "CCCD")
                {
                    newBackFileName = IdentityDocumentStorage.Save(backDocument);
                }

                if (existing == null)
                {
                    existing = new PasswordResetVerification { UserId = user.UserId };
                    db.PasswordResetVerifications.Add(existing);
                }

                existing.DocumentNumberHash = identityVerification.DocumentNumberHash;
                existing.DocumentType = DocumentType;
                existing.FrontFileName = newFrontFileName;
                existing.BackFileName = newBackFileName;
                existing.Status = "Pending";
                existing.ReviewNote = null;
                existing.SubmittedAt = DateTime.Now;
                existing.ReviewedAt = null;
                existing.ReviewedByUserId = null;
                existing.ResetOtpHash = null;
                existing.ResetOtpExpiresAt = null;
                db.SaveChanges();
            }
            catch (InvalidDataException ex)
            {
                IdentityDocumentStorage.Delete(newFrontFileName);
                IdentityDocumentStorage.Delete(newBackFileName);
                ModelState.AddModelError("", ex.Message);
                return View();
            }
            catch
            {
                IdentityDocumentStorage.Delete(newFrontFileName);
                IdentityDocumentStorage.Delete(newBackFileName);
                throw;
            }

            try
            {
                IdentityDocumentStorage.Delete(oldFrontFileName);
                IdentityDocumentStorage.Delete(oldBackFileName);
            }
            catch (IOException ex)
            {
                System.Diagnostics.Trace.TraceError("Could not delete replaced password-reset documents: " + ex);
                TempData["Warning"] = "Không xóa được ảnh giấy tờ khôi phục cũ. Quản trị viên cần kiểm tra App_Data/IdentityDocuments.";
            }
            catch (InvalidDataException ex)
            {
                System.Diagnostics.Trace.TraceError("Could not delete replaced password-reset documents: " + ex);
                TempData["Warning"] = "Có ảnh giấy tờ khôi phục cũ không hợp lệ cần được kiểm tra.";
            }

            try
            {
                IdentityDocumentStorage.DeleteResetOtp(existing.PasswordResetVerificationId);
            }
            catch (IOException ex)
            {
                System.Diagnostics.Trace.TraceError("Could not delete stale reset OTP: " + ex);
                TempData["Warning"] = "Không xóa được OTP cũ; OTP cũ không còn hiệu lực trong hệ thống.";
            }
            Session["ResetVerificationId"] = existing.PasswordResetVerificationId;
            Session["Reset_Email"] = user.Email;
            return RedirectToAction("ForgotPassword");
        }

        [HttpGet]
        public ActionResult ConfirmResetOtp()
        {
            int? verificationId = Session["ResetVerificationId"] as int?;
            if (!verificationId.HasValue)
            {
                return RedirectToAction("ForgotPassword");
            }

            var verification = db.PasswordResetVerifications.Find(verificationId.Value);
            if (verification == null)
            {
                Session.Remove("ResetVerificationId");
                Session.Remove("Reset_Email");
                return RedirectToAction("ForgotPassword");
            }

            PopulateResetOtpView(verification);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmResetOtp(string otpInput, string newPassword, string confirmPassword)
        {
            int? verificationId = Session["ResetVerificationId"] as int?;
            if (!verificationId.HasValue)
            {
                return RedirectToAction("ForgotPassword");
            }

            var verification = db.PasswordResetVerifications.Find(verificationId.Value);
            if (verification == null)
            {
                Session.Remove("ResetVerificationId");
                Session.Remove("Reset_Email");
                return RedirectToAction("ForgotPassword");
            }
            PopulateResetOtpView(verification);

            if (verification.Status != "Approved" ||
                !verification.ResetOtpExpiresAt.HasValue ||
                DateTime.Now > verification.ResetOtpExpiresAt.Value ||
                string.IsNullOrWhiteSpace(verification.ResetOtpHash))
            {
                ModelState.AddModelError("", verification.Status == "Pending"
                    ? "Yêu cầu đang chờ ADMIN/MOD xác minh giấy tờ."
                    : "Mã OTP chưa được cấp hoặc đã hết hạn. Vui lòng gửi yêu cầu xác minh mới.");
                return View();
            }

            bool otpMatches;
            try
            {
                otpMatches = IdentityDocumentStorage.VerifyDocumentNumber(
                    otpInput == null ? null : "OTP" + otpInput.Trim(),
                    verification.ResetOtpHash);
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Trace.TraceError("Identity document HMAC configuration error: " + ex);
                ModelState.AddModelError("", "Không thể kiểm tra mã OTP do cấu hình bảo mật chưa hợp lệ.");
                return View();
            }
            if (!otpMatches)
            {
                ModelState.AddModelError("", "Mã OTP không chính xác!");
                return View();
            }

            // Kiểm tra độ mạnh mật khẩu mới (issue #130)
            if (!PasswordPolicy.IsValid(newPassword))
            {
                ModelState.AddModelError("", PasswordPolicy.RuleMessage);
                return View();
            }

            // Kiểm tra khớp mật khẩu
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp!");
                return View();
            }

            var user = db.Users.Find(verification.UserId);
            if (user != null)
            {
                // FIX BUG: PHẢI DÙNG MD5 HASH ĐỂ ĐỒNG BỘ VỚI HÀM LOGIN
                user.Password = Encryptor.MD5Hash(newPassword);
                db.SaveChanges();

                verification.Status = "Completed";
                verification.ResetOtpHash = null;
                verification.ResetOtpExpiresAt = null;
                db.SaveChanges();
                IdentityDocumentStorage.DeleteResetOtp(verification.PasswordResetVerificationId);

                Session.Remove("ResetVerificationId");
                Session.Remove("Reset_Email");
                Session.Remove("DemoOtp");
                Session.Remove("DemoOtpExpireTime");

                TempData["Success"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập bằng mật khẩu mới.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Không tìm thấy tài khoản để cập nhật!");
            return View();
        }

        private void PopulateResetOtpView(PasswordResetVerification verification)
        {
            ViewBag.Email = Session["Reset_Email"];
            ViewBag.ResetVerificationStatus = verification.Status;
            ViewBag.ResetVerificationNote = verification.ReviewNote;

            bool otpExpired = !verification.ResetOtpExpiresAt.HasValue ||
                DateTime.Now > verification.ResetOtpExpiresAt.Value ||
                string.IsNullOrWhiteSpace(verification.ResetOtpHash);
            ViewBag.ResetOtpExpired = otpExpired;
            if (otpExpired)
            {
                IdentityDocumentStorage.DeleteResetOtp(verification.PasswordResetVerificationId);
                return;
            }

            try
            {
                ViewBag.ResetOtpForCustomer =
                    IdentityDocumentStorage.ReadResetOtp(verification.PasswordResetVerificationId);
            }
            catch (IOException ex)
            {
                System.Diagnostics.Trace.TraceError("Could not read protected reset OTP: " + ex);
                ModelState.AddModelError("", "Không thể đọc OTP đã bảo vệ. Vui lòng liên hệ quản trị viên.");
            }
            catch (CryptographicException ex)
            {
                System.Diagnostics.Trace.TraceError("Could not unprotect stored reset OTP: " + ex);
                ModelState.AddModelError("", "Không thể giải mã OTP đã bảo vệ. Vui lòng liên hệ quản trị viên.");
            }
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

            if (TempData["Success"] != null)
            {
                ViewBag.Success = TempData["Success"];
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

            // Kiem tra do manh mat khau moi (issue #130)
            if (!PasswordPolicy.IsValid(model.NewPassword))
            {
                ModelState.AddModelError("NewPassword", PasswordPolicy.RuleMessage);
                return View(model);
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

            TempData["Success"] = "Đổi mật khẩu thành công! Từ giờ hãy dùng mật khẩu mới cho lần đăng nhập tiếp theo.";
            return RedirectToAction("ChangePassword");
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

// ============ Issue #114: Gui OTP vao kenh Chat noi bo ============
        // OTP cho dang ky / lay lai mat khau duoc ghi nhu 1 tin nhan cua "He thong"
        // vao thread chat. Khach doc duoc bang: dang nhap lai /Chat, hoac tra cuu
        // bang Email + So dien thoai (khi chua dang nhap duoc).
        private void SendOtpToChat(string email, string phone, string content, string senderName, int? userId = null)
        {
            try
            {
                using (var chatDao = new ChatMessageDao())
                {
                    chatDao.Insert(new ChatMessage
                    {
                        UserId = userId, // null khi dang ky chua xac thuc; khach quen mat khau co UserId ngay
                        Email = email,
                        Phone = phone,
                        Sender = "Hệ thống",
                        Content = content,
                        IsSystem = true,
                        Status = true
                    });
                }
            }
            catch (Exception)
            {
                // Chat la kenh phu: loi ghi chat khong duoc lam treo luong OTP chinh
            }
        }

        private void StoreDemoOtpOnHome(string otp, DateTime expireTime)
        {
            if (MailHelper.LastSentViaDevFallback)
            {
                Session["DemoOtp"] = otp;
                Session["DemoOtpExpireTime"] = expireTime;
                return;
            }

            Session.Remove("DemoOtp");
            Session.Remove("DemoOtpExpireTime");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Areas.Admin.Models;
using Models.DAO;
using Models.EF;
using WebsiteNoiThat.Common;
using System.Web.Security;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class LoginController : Controller
    {
        // Kết xuất dữ liệu dùng chung cho trang quên mật khẩu quản trị (issue #74)
        private readonly DBNoiThat db = new DBNoiThat();

        // GET: Admin/Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var dao = new UserDao();
                var result = dao.Login(model.Username, Encryptor.MD5Hash(model.Passwword), true);
                if (result == 1)
                {
                    // User có thể nhập Tên đăng nhập, Email hoặc SĐT (issue #74) -> lấy đúng user từ DB
                    var user = dao.GetUserForLogin(model.Username);
                    var userSession = new UserLogin();
                    userSession.Username = user.Username;
                    userSession.UserId = user.UserId;
                    Session["UserId"] = user.UserId.ToString();
                    userSession.GroupId = user.GroupId;
                    var listCredentials = dao.GetListCredentials(user.Username);
                    Session.Add(Commoncontent.SESSION_CREDENTIALS, listCredentials);
                    Session.Add(Commoncontent.user_sesion_admin, userSession);
                    return RedirectToAction("Index", "Home");
                }
                else if (result == 0)
                {
                    ModelState.AddModelError("", "Tài Khoản không tồn tại!");

                }
                else if (result == -1)
                {
                    ModelState.AddModelError("", "Tài Khoản đang bị khóa!");
                }
                else if (result == -2)
                {
                    ModelState.AddModelError("", "Mật khẩu không đúng!");
                }
                else if (result == -3)
                {
                    ModelState.AddModelError("", "Tài khoản của bạn không có quyền đăng nhập.");
                }
            }

            return View("Index");
        }
        public ActionResult Logout()
        {
            Session[Commoncontent.user_sesion_admin] = null;
            Session[Commoncontent.SESSION_CREDENTIALS] = null;
           FormsAuthentication.SignOut();
            return RedirectToAction("Index","Login");
        }

        // ==================== QUÊN MẬT KHẨU QUẢN TRỊ (issue #74) ====================
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string Email)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ModelState.AddModelError("", "Vui lòng nhập địa chỉ email!");
                return View();
            }

            // Chỉ tài khoản quản trị (ADMIN / MOD) mới được đặt lại mật khẩu qua màn hình này,
            // tài khoản khách hàng thường phải dùng trang quên mật khẩu phía khách (RegisterAndLogin)
            var user = db.Users.FirstOrDefault(x => x.Email.ToLower() == Email.Trim().ToLower()
                && (x.GroupId == CommonConstant.ADMIN_GROUP || x.GroupId == CommonConstant.MOD_GROUP));
            if (user == null)
            {
                ModelState.AddModelError("", "Email này không thuộc tài khoản quản trị nào!");
                return View();
            }
            if (user.Status != true)
            {
                ModelState.AddModelError("", "Tài khoản đang bị khóa! Vui lòng liên hệ quản trị viên khác.");
                return View();
            }

            // Tạo mã OTP, lưu phiên 5 phút (tên session riêng để không đụng luồng quên mật khẩu phía khách)
            string otp = MailHelper.GenerateOTP();
            Session["AdminReset_OTP"] = otp;
            Session["AdminReset_Email"] = user.Email;
            Session["AdminReset_ExpireTime"] = DateTime.Now.AddMinutes(5);

            string body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e3e6f0; border-radius: 8px; max-width: 500px;'>
                    <h3 style='color: #4e73df; margin-top: 0;'>Khôi phục mật khẩu quản trị</h3>
                    <p>Xin chào <b>{user.Name ?? user.Username}</b>,</p>
                    <p>Bạn (hoặc ai đó) vừa yêu cầu đặt lại mật khẩu quản trị cho tài khoản <b>{user.Username}</b>. Mã OTP xác thực là:</p>
                    <div style='text-align: center; margin: 20px 0;'>
                        <span style='font-size: 28px; font-weight: bold; color: #e74a3b; letter-spacing: 6px; background: #f8f9fc; padding: 10px 20px; border-radius: 6px; border: 1px dashed #d1d3e2;'>{otp}</span>
                    </div>
                    <p style='color: #858796; font-size: 13px;'>Mã này có hiệu lực trong <b>5 phút</b>. Vui lòng không cung cấp mã này cho người khác.</p>
                </div>";

            bool sent = MailHelper.SendMail(user.Email, "Mã OTP khôi phục mật khẩu quản trị - Website Nội Thất", body);
            if (!sent)
            {
                ModelState.AddModelError("", "Không thể gửi email. Vui lòng kiểm tra cấu hình mail hoặc thử lại sau!");
                return View();
            }

            return RedirectToAction("ConfirmResetOtp");
        }

        [HttpGet]
        public ActionResult ConfirmResetOtp()
        {
            if (Session["AdminReset_Email"] == null)
            {
                return RedirectToAction("ForgotPassword");
            }
            ViewBag.Email = Session["AdminReset_Email"].ToString();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmResetOtp(string otpInput, string newPassword, string confirmPassword)
        {
            string sessionOtp = Session["AdminReset_OTP"] as string;
            string email = Session["AdminReset_Email"] as string;
            DateTime? expireTime = Session["AdminReset_ExpireTime"] as DateTime?;

            ViewBag.Email = email;

            // Hết hạn phiên / OTP
            if (expireTime == null || DateTime.Now > expireTime)
            {
                ModelState.AddModelError("", "Mã OTP đã hết hạn! Vui lòng yêu cầu lại mã mới.");
                return View();
            }

            // Sai mã OTP
            if (string.IsNullOrWhiteSpace(otpInput) || sessionOtp != otpInput.Trim())
            {
                ModelState.AddModelError("", "Mã OTP không đúng! Vui lòng kiểm tra lại email.");
                return View();
            }

            // Mật khẩu mới
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu mới phải có tối thiểu 6 ký tự!");
                return View();
            }
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Xác nhận mật khẩu không khớp!");
                return View();
            }

            // Chỉ cập nhật đúng tài khoản quản trị ứng với email đã xác thực OTP
            var user = db.Users.FirstOrDefault(x => x.Email == email
                && (x.GroupId == CommonConstant.ADMIN_GROUP || x.GroupId == CommonConstant.MOD_GROUP));
            if (user != null)
            {
                // Lưu MD5 Hash cho đồng bộ với hàm Login
                user.Password = Encryptor.MD5Hash(newPassword);
                db.SaveChanges();

                // Xóa sạch Session sau khi hoàn thành
                Session.Remove("AdminReset_OTP");
                Session.Remove("AdminReset_Email");
                Session.Remove("AdminReset_ExpireTime");

                TempData["Success"] = "Đặt lại mật khẩu quản trị thành công! Vui lòng đăng nhập bằng mật khẩu mới.";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Không tìm thấy tài khoản quản trị để cập nhật!");
            return View();
        }
    }
}
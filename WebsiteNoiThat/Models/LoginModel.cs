using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteNoiThat.Models
{
    public class LoginModel
    {
        [DisplayName("Tài khoản (Tên đăng nhập / Email / SĐT)")]
        [Required(ErrorMessage = "Bạn phải nhập tài khoản!")]
        public string UserName { get; set; }

        [DisplayName("Mật khẩu")]
        [Required(ErrorMessage = "Bạn phải nhập mật khẩu!")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
        // Cần thiết để BotDetect bind và validate mã captcha từ form
        public string CaptchaCode { get; set; }
        
    }
}
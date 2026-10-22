using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteNoiThat.Models
{
    public class RegisterModel
    {
       

        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Yêu cầu nhập tên đăng nhập")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự.")]
        [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Tên đăng nhập chỉ gồm chữ, số, dấu chấm và gạch dưới.")]
        public string UserName { set; get; }

        [Display(Name = "Mật khẩu")]
        [StringLength(32, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 32 ký tự.")]
        [Required(ErrorMessage = "Yêu cầu nhập mật khẩu")]
        public string Password { set; get; }

        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Xác nhận mật khẩu không đúng.")]
        public string ConfirmPassword { set; get; }

        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "Yêu cầu nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
        public string Name { set; get; }

        [Display(Name = "Địa chỉ")]
        [StringLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự.")]
        public string Address { set; get; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Yêu cầu nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ (ví dụ: ten@gmail.com).")]
        public string Email { set; get; }

        [Display(Name = "Điện thoại")]
        [Required(ErrorMessage = "Yêu cầu nhập số điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ (VD: 0901234567).")]
        public string Phone { set; get; }

        public int CartId { get; set; }
    }
}
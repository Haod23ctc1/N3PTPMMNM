using System.ComponentModel.DataAnnotations;

namespace WebsiteNoiThat.Models
{
    public class ChangePasswordModel
    {
        [Display(Name = "Mật khẩu cũ")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Display(Name = "Mật khẩu mới")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "Độ dài mật khẩu ít nhất 4 ký tự.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Xác nhận mật khẩu mới")]
        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không đúng.")]
        public string ConfirmPassword { get; set; }
    }
}

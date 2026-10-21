using System.ComponentModel.DataAnnotations;

namespace WebsiteNoiThat.Models
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
        public string Email { get; set; }
    }
}
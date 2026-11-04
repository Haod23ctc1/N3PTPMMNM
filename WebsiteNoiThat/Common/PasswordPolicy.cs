using System.Linq;

namespace WebsiteNoiThat.Common
{
    // Chinh sach do manh mat khau dung chung cho moi trang nhap/doi mat khau
    // (issue #130): dang ky, doi mat khau, lay lai mat khau, them/sua nguoi dung
    // o ca phan khach hang va phan quan tri.
    public static class PasswordPolicy
    {
        public const int MinimumLength = 8;
        public const int MaximumLength = 32;

        public const string RuleMessage = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm cả chữ cái và số.";

        // Mat khau hop le: tu 8 den 32 ky tu, co it nhat 1 chu cai va 1 chu so
        public static bool IsValid(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < MinimumLength || password.Length > MaximumLength)
            {
                return false;
            }
            return password.Any(char.IsLetter) && password.Any(char.IsDigit);
        }
    }
}

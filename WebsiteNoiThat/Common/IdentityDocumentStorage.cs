using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;

namespace WebsiteNoiThat.Common
{
    public static class IdentityDocumentStorage
    {
        private const int MaxFileSize = 1572864;
        private const string ResetOtpProtectionPurpose = "WebsiteNoiThat.PasswordResetOtp.v1";

        public static void SaveResetOtp(int verificationId, string otp)
        {
            if (verificationId <= 0)
            {
                throw new ArgumentOutOfRangeException("verificationId");
            }
            if (string.IsNullOrWhiteSpace(otp))
            {
                throw new ArgumentException("OTP cannot be empty.", "otp");
            }

            byte[] protectedBytes = MachineKey.Protect(
                Encoding.UTF8.GetBytes(otp), ResetOtpProtectionPurpose);
            if (protectedBytes == null)
            {
                throw new CryptographicException("Could not protect reset OTP.");
            }

            string directory = HttpContext.Current.Server.MapPath("~/App_Data/ResetOtps");
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(GetResetOtpPath(directory, verificationId), protectedBytes);
        }

        public static string ReadResetOtp(int verificationId)
        {
            if (verificationId <= 0)
            {
                throw new ArgumentOutOfRangeException("verificationId");
            }

            string directory = HttpContext.Current.Server.MapPath("~/App_Data/ResetOtps");
            string path = GetResetOtpPath(directory, verificationId);
            if (!File.Exists(path))
            {
                return null;
            }

            byte[] otpBytes = MachineKey.Unprotect(
                File.ReadAllBytes(path), ResetOtpProtectionPurpose);
            if (otpBytes == null)
            {
                throw new CryptographicException("Could not unprotect reset OTP.");
            }
            return Encoding.UTF8.GetString(otpBytes);
        }

        public static void DeleteResetOtp(int verificationId)
        {
            if (verificationId <= 0)
            {
                throw new ArgumentOutOfRangeException("verificationId");
            }

            string directory = HttpContext.Current.Server.MapPath("~/App_Data/ResetOtps");
            string path = GetResetOtpPath(directory, verificationId);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static string ComputeDocumentNumberHash(string documentNumber)
        {
            string key = ConfigurationManager.AppSettings["IdentityDocumentHmacKey"];
            if (string.IsNullOrWhiteSpace(key))
            {
                key = Environment.GetEnvironmentVariable("IdentityDocumentHmacKey");
            }
            if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
            {
                throw new InvalidOperationException(
                    "IdentityDocumentHmacKey must be configured as a secret of at least 32 UTF-8 bytes.");
            }

            string normalized = NormalizeDocumentNumber(documentNumber);
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(normalized));
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }

        public static bool VerifyDocumentNumber(string documentNumber, string expectedHash)
        {
            if (string.IsNullOrWhiteSpace(expectedHash))
            {
                return false;
            }

            byte[] expected;
            try
            {
                expected = HexToBytes(expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (OverflowException)
            {
                return false;
            }

            byte[] actual;
            try
            {
                actual = HexToBytes(ComputeDocumentNumberHash(documentNumber));
            }
            catch (ArgumentException)
            {
                return false;
            }

            if (expected.Length != actual.Length)
            {
                return false;
            }

            int difference = 0;
            for (int i = 0; i < expected.Length; i++)
            {
                difference |= expected[i] ^ actual[i];
            }
            return difference == 0;
        }

        public static string Save(HttpPostedFileBase upload)
        {
            if (upload == null || upload.ContentLength <= 0)
            {
                throw new InvalidDataException("Vui lòng tải ảnh giấy tờ lên.");
            }

            if (upload.ContentLength > MaxFileSize)
            {
                throw new InvalidDataException("Mỗi ảnh giấy tờ không được vượt quá 1,5 MB.");
            }

            string extension = GetValidatedExtension(upload.InputStream);
            string fileName = Guid.NewGuid().ToString("N") + extension;
            string directory = HttpContext.Current.Server.MapPath("~/App_Data/IdentityDocuments");
            Directory.CreateDirectory(directory);
            upload.InputStream.Position = 0;
            upload.SaveAs(Path.Combine(directory, fileName));
            return fileName;
        }

        public static byte[] Read(string fileName)
        {
            string path = GetPath(fileName);
            return File.ReadAllBytes(path);
        }

        public static void Delete(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            string path = GetPath(fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(GetSafeFileName(fileName));
            return extension == ".png" ? "image/png" : "image/jpeg";
        }

        private static string GetValidatedExtension(Stream stream)
        {
            if (stream == null || !stream.CanSeek)
            {
                throw new InvalidDataException("Không thể đọc ảnh giấy tờ.");
            }

            long originalPosition = stream.Position;
            var header = new byte[8];
            int count = stream.Read(header, 0, header.Length);
            stream.Position = originalPosition;

            if (count >= 8 &&
                header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            {
                return ".png";
            }

            if (count >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            {
                return ".jpg";
            }

            throw new InvalidDataException("Chỉ chấp nhận ảnh JPEG hoặc PNG.");
        }

        private static string NormalizeDocumentNumber(string documentNumber)
        {
            if (string.IsNullOrWhiteSpace(documentNumber))
            {
                throw new ArgumentException("Vui lòng nhập số giấy tờ tùy thân.");
            }

            string normalized = new string(documentNumber
                .Where(char.IsLetterOrDigit)
                .Select(char.ToUpperInvariant)
                .ToArray());
            if (normalized.Length < 5 || normalized.Length > 20)
            {
                throw new ArgumentException("Số giấy tờ phải có từ 5 đến 20 ký tự chữ hoặc số.");
            }
            return normalized;
        }

        private static byte[] HexToBytes(string value)
        {
            if (value.Length != 64 || value.Length % 2 != 0)
            {
                throw new FormatException("Invalid document hash.");
            }

            var bytes = new byte[value.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(value.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        private static string GetPath(string fileName)
        {
            string safeName = GetSafeFileName(fileName);
            string directory = HttpContext.Current.Server.MapPath("~/App_Data/IdentityDocuments");
            return Path.Combine(directory, safeName);
        }

        private static string GetResetOtpPath(string directory, int verificationId)
        {
            return Path.Combine(directory, verificationId.ToString(
                System.Globalization.CultureInfo.InvariantCulture) + ".bin");
        }

        private static string GetSafeFileName(string fileName)
        {
            string safeName = Path.GetFileName(fileName ?? string.Empty);
            string baseName = Path.GetFileNameWithoutExtension(safeName);
            string extension = Path.GetExtension(safeName);
            Guid id;
            if (safeName != fileName || !Guid.TryParseExact(baseName, "N", out id) ||
                (extension != ".jpg" && extension != ".png"))
            {
                throw new InvalidDataException("Tên tệp giấy tờ không hợp lệ.");
            }

            return safeName;
        }
    }
}

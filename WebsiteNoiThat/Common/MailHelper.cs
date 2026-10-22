using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace WebsiteNoiThat.Common
{
    public class MailHelper
    {
        /// <summary>
        /// Hàm tự động sinh mã OTP ngẫu nhiên (mặc định 6 số)
        /// </summary>
        public static string GenerateOTP(int length = 6)
        {
            Random rand = new Random();
            int min = (int)Math.Pow(10, length - 1);       // 100000
            int max = (int)Math.Pow(10, length) - 1;       // 999999
            return rand.Next(min, max).ToString();
        }

        /// <summary>
        /// Gửi Email hỗ trợ UTF-8, tự động bắt lỗi và trả về true/false
        /// </summary>
        public static bool SendMail(string toEmailAddress, string subject, string content)
        {
        try
        {
        // BẮT BUỘC: Ép hệ thống sử dụng giao thức bảo mật TLS 1.2 để bắt tay với Resend
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

        var fromEmailAddress = ConfigurationManager.AppSettings["FromEmailAddress"];
        var fromEmailDisplayName = ConfigurationManager.AppSettings["FromEmailDisplayName"] ?? "Website Nội Thất";
        var fromEmailPassword = ConfigurationManager.AppSettings["FromEmailPassword"]; // Đây chính là API Key re_...
        var smtpHost = ConfigurationManager.AppSettings["SMTPHost"] ?? "://resend.com";
        var smtpPortStr = ConfigurationManager.AppSettings["SMTPPort"];
        var sslStr = ConfigurationManager.AppSettings["EnabledSSL"];

        // Xử lý cổng Port an toàn (mặc định là 587 nếu chưa cấu hình)
        int smtpPort = 587;
        if (!string.IsNullOrEmpty(smtpPortStr))
        {
            int.TryParse(smtpPortStr, out smtpPort);
        }

        // Xử lý SSL an toàn (mặc định là true nếu chưa cấu hình)
        bool enabledSsl = true;
        if (!string.IsNullOrEmpty(sslStr))
        {
            bool.TryParse(sslStr, out enabledSsl);
        }

        // Thiết lập nội dung thư
        using (var message = new MailMessage(new MailAddress(fromEmailAddress, fromEmailDisplayName), new MailAddress(toEmailAddress)))
        {
            message.Subject = subject;
            message.Body = content;
            message.IsBodyHtml = true;

            // Chống lỗi hiển thị dấu Tiếng Việt
            message.SubjectEncoding = Encoding.UTF8;
            message.BodyEncoding = Encoding.UTF8;

            // Thiết lập kết nối SMTP
            using (var client = new SmtpClient())
            {
                client.Host = smtpHost;
                client.Port = smtpPort;
                client.EnableSsl = enabledSsl;
                client.UseDefaultCredentials = false;

                // SỬA QUAN TRỌNG: Username kết nối đến Resend bắt buộc cố định là chữ "resend" viết thường!
                client.Credentials = new NetworkCredential("resend", fromEmailPassword);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                client.Send(message);
            }
        }

        return true; // Gửi thành công
    }
        catch (Exception ex)
        {
            // In chi tiết lỗi ra cửa sổ Output của Visual Studio để dễ kiểm tra
            System.Diagnostics.Debug.WriteLine(">>> LỖI GỬI EMAIL CHI TIẾT: " + ex.ToString());
            return false; // Gửi thất bại
        }
}

    }
}
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
        /// Chế độ dev: lưu email ra file trong App_Data\MailOut khi SMTP gửi thất bại,
        /// để luồng OTP (đăng ký / quên mật khẩu) vẫn chạy được trên máy bất kỳ ai
        /// (thiếu API key, SMTP bị chặn, hoặc Resend chưa xác minh tên miền).
        /// Khi triển khai thật: cài "MailDevFallback" = "false" trong Web.config.
        /// </summary>
        private static void SaveMailToFile(string toEmailAddress, string subject, string content)
        {
            string folder = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/MailOut");
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }

            string stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string safeTo = string.Join("_", (toEmailAddress ?? "unknown").Split(System.IO.Path.GetInvalidFileNameChars()));
            string safeSubject = string.Join("_", (subject ?? "mail").Split(System.IO.Path.GetInvalidFileNameChars()));
            string file = System.IO.Path.Combine(folder, stamp + "_" + safeTo + "_" + safeSubject + ".html");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<!-- " + "To: " + toEmailAddress + " -->");
            sb.AppendLine("<!-- " + "Subject: " + subject + " -->");
            sb.AppendLine("<!-- " + "Time: " + DateTime.Now + " -->");
            sb.Append(content);
            System.IO.File.WriteAllText(file, sb.ToString(), Encoding.UTF8);

            System.Diagnostics.Debug.WriteLine(">>> MAIL DEV FALLBACK: đã lưu email vào " + file);
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
        var smtpHost = ConfigurationManager.AppSettings["SMTPHost"] ?? "smtp.resend.com";
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

            // Fallback chế độ dev: SMTP gửi thất bại (lỗi cấu hình, API key, chưa xác minh
            // tên miền Resend, mạng chặn cổng 587...) -> lưu email ra file trong
            // App_Data\MailOut và coi như gửi thành công để luồng OTP không bị tắc.
            // Bật/tắt qua cấu hình "MailDevFallback" trong Web.config.
            bool devFallback = true;
            string fallbackCfg = ConfigurationManager.AppSettings["MailDevFallback"];
            if (!string.IsNullOrEmpty(fallbackCfg))
            {
                bool.TryParse(fallbackCfg, out devFallback);
            }

            if (devFallback)
            {
                try
                {
                    SaveMailToFile(toEmailAddress, subject, content);
                    return true; // Đã "gửi" qua kênh file trong môi trường dev
                }
                catch (Exception fileEx)
                {
                    System.Diagnostics.Debug.WriteLine(">>> LỖI LƯU EMAIL FALLBACK: " + fileEx.ToString());
                    return false;
                }
            }

            return false; // Gửi thất bại (không dùng fallback: môi trường triển khai thật)
        }
}

    }
}
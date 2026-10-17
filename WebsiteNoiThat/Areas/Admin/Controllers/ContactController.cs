using Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Common;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class ContactController : HomeController
    {
        // GET: Admin/Contact
        DBNoiThat db = new DBNoiThat();

        public new ActionResult Index()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            if (session != null)
            {
                ViewBag.username = session.Username;
            }

            var model = db.Contacts.OrderByDescending(c => c.ContactId).ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reply(int id, string replyContent)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            if (session != null)
            {
                ViewBag.username = session.Username;
            }

            var contact = db.Contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(replyContent))
            {
                TempData["Warning"] = "Vui lòng nhập nội dung phản hồi cho khách hàng.";
                return RedirectToAction("Index");
            }

            // Đánh dấu đã xử lý
            contact.Status = true;

            // Lưu nội dung phản hồi vào lịch sử
            string adminName = session != null ? session.Username : "Ban Quản Trị";
            contact.Content = contact.Content + $"\n\n[Đã phản hồi bởi {adminName} lúc {DateTime.Now:dd/MM/yyyy HH:mm}]:\n{replyContent.Trim()}";
            db.SaveChanges();

            // Gửi email cho khách hàng
            if (!string.IsNullOrWhiteSpace(contact.EmailCC))
            {
                try
                {
                    string subject = $"[Nội Thất An Huy] Phản hồi yêu cầu hỗ trợ #{contact.ContactId}";
                    string htmlBody = $@"
                        <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <h2 style='color: #d9534f;'>Studio Nội Thất An Huy</h2>
                            <p>Kính gửi Quý khách,</p>
                            <p>Bộ phận Chăm sóc Khách hàng đã tiếp nhận và hoàn tất xử lý yêu cầu hỗ trợ của Quý khách (Mã yêu cầu: <strong>#{contact.ContactId}</strong>).</p>
                            
                            <div style='background-color: #f8f9fa; border-left: 4px solid #17a2b8; padding: 12px; margin: 15px 0;'>
                                <strong style='color: #17a2b8;'>Nội dung yêu cầu của Quý khách:</strong>
                                <p style='margin: 8px 0 0 0;'>{HttpUtility.HtmlEncode(contact.Content.Split(new[] { "\n\n[Đã phản hồi" }, StringSplitOptions.None)[0]).Replace("\n", "<br/>")}</p>
                            </div>

                            <div style='background-color: #e8f4fd; border-left: 4px solid #28a745; padding: 12px; margin: 15px 0;'>
                                <strong style='color: #28a745;'>Nội dung hỗ trợ / phản hồi từ Chúng tôi:</strong>
                                <p style='margin: 8px 0 0 0;'>{HttpUtility.HtmlEncode(replyContent).Replace("\n", "<br/>")}</p>
                            </div>

                            <p>Nếu Quý khách có thêm bất kỳ thắc mắc nào khác, xin vui lòng phản hồi email này hoặc liên hệ hotline: <strong>01649.629.629</strong>.</p>
                            <p>Xin trân trọng cảm ơn Quý khách đã tin tưởng và đồng hành cùng <strong>Nội Thất An Huy</strong>!</p>
                            <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;' />
                            <small style='color: #777;'>Địa chỉ: Ngõ 34 - Phú Kiều - Kiều Mai | Hotline: 01649.629.629</small>
                        </div>";

                    new MailHelper().SendMail(contact.EmailCC.Trim(), subject, htmlBody);
                    TempData["Success"] = $"Đã hoàn thành hỗ trợ và gửi email phản hồi thành công đến {contact.EmailCC}!";
                }
                catch (Exception ex)
                {
                    TempData["Warning"] = $"Đã lưu trạng thái hoàn thành hỗ trợ #{contact.ContactId}, nhưng gửi email không thành công (Lý do: {ex.Message}). Vui lòng kiểm tra lại cấu hình SMTP!";
                }
            }
            else
            {
                TempData["Success"] = $"Đã hoàn thành hỗ trợ #{contact.ContactId}! (Không tìm thấy địa chỉ email để gửi thư).";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var contact = db.Contacts.Find(id);
            if (contact != null)
            {
                db.Contacts.Remove(contact);
                db.SaveChanges();
                TempData["Success"] = "Đã xóa yêu cầu hỗ trợ thành công.";
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
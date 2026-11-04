using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Models.DAO;
using Models.EF;
using WebsiteNoiThat.Common;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    // Quan tri phan ho tro khach hang qua Chat noi bo (issue #122):
    // - Xem danh sach cac thread chat cua khach (bao gom thread khach chua xac thuc)
    // - Xem noi dung hoi thoai va phan hoi (tin nhan admin hien ben khach la ben trai)
    // Chu y: ke thua Controller truc tiep (KHONG ke thua HomeController) vi neu ke thua
    // thi action Index() cua HomeController bi tham chieu -> AmbiguousMatchException
    // (bug dang ton tai voi ContactController cung pattern `public new`).
    public class SupportChatController : Controller
    {
        ChatMessageDao dao = new ChatMessageDao();

        // GET /Admin/SupportChat
        [HasCredential(RoleId = "VIEW_ADMIN")]
        public ActionResult Index()
        {
            var session = (UserLogin)Session[Commoncontent.user_sesion_admin];
            if (session == null)
            {
                return Redirect("~/Admin/Login");
            }
            ViewBag.username = session.Username;
            return View(dao.ListThreads());
        }

        // GET /Admin/SupportChat/Details?email=...&phone=... - 1 thread cu the
        [HasCredential(RoleId = "VIEW_ADMIN")]
        public ActionResult Details(string email, string phone)
        {
            var session = (UserLogin)Session[Commoncontent.user_sesion_admin];
            if (session != null)
            {
                ViewBag.username = session.Username;
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("Index");
            }

            var dtb = ChatMessageDao.NormalizePhone(phone);
            var mail = email.Trim().ToLower();

            // Tin nhan cua thread: khop Email (co them Phone de phan biet thread khach)
            var msgs = dao.ListAll()
                          .Where(m => (m.Email ?? string.Empty).Trim().ToLower() == mail)
                          .Where(m => string.IsNullOrEmpty(dtb) || m.UserId.HasValue || m.Phone == dtb)
                          .ToList();

            ViewBag.ThreadEmail = email.Trim();
            ViewBag.ThreadPhone = dtb;
            return View(msgs);
        }

        // POST /Admin/SupportChat/Reply - admin phan hoi tin nhan khach
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredential(RoleId = "VIEW_ADMIN")]
        public ActionResult Reply(string email, string phone, string content)
        {
            var session = (UserLogin)Session[Commoncontent.user_sesion_admin];
            var adminName = session != null ? session.Username : "Quan tri";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(content))
            {
                TempData["Warning"] = "Vui lòng nhập nội dung phản hồi!";
                return RedirectToAction("Details", new { email, phone });
            }
            if (content.Length > ChatMessage.MAX_NVARCHAR)
            {
                content = content.Substring(0, ChatMessage.MAX_NVARCHAR);
            }

            var dtb = ChatMessageDao.NormalizePhone(phone);
            var mail = email.Trim().ToLower();

            using (var db = new DBNoiThat())
            {
                // Neu thread gan voi user thi luu luon UserId (khanh login thay luon)
                var user = db.Users.FirstOrDefault(u => u.Email.ToLower() == mail);
                dao.Insert(new ChatMessage
                {
                    UserId = user != null ? (int?)user.UserId : null,
                    Email = mail,
                    Phone = dtb,
                    Sender = "Hỗ trợ viên " + adminName,
                    Content = content,
                    IsSystem = true, // hien ben trai trong chat cua khach (ben he thong)
                    Status = true
                });
            }

            TempData["Success"] = "Đã gửi phản hồi cho khách hàng!";
            return RedirectToAction("Details", new { email, phone });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dao.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

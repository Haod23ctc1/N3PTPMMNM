using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Models.DAO;
using Models.EF;
using WebsiteNoiThat.Common;

namespace WebsiteNoiThat.Controllers
{
    // Chat noi bo tren site (issue #114):
    // - Khach dang nhap xem thread cua minh o /Chat
    // - Khach CHUA dang nhap tra cuu chat bang Email + Phone (dung de doc OTP
    //   khi dang ky / lay lai mat khau ma mail khong gui duoc)
    // - He thong tu dong nhdua OTP vao chat khi dang ky / lay lai mat khau
    public class ChatController : Controller
    {
        ChatMessageDao dao = new ChatMessageDao();

        // GET /Chat - xem chat (can dang nhap, hoac tra cuu bang Email + Phone)
        public ActionResult Index()
        {
            ViewBag.ChatError = TempData["ChatError"];
            var session = (UserLogin)Session[Commoncontent.user_sesion];
            if (session != null)
            {
                using (var db = new DBNoiThat())
                {
                    var user = db.Users.Find(session.UserId);
                    if (user != null)
                    {
                        ViewBag.ThreadEmail = user.Email;
                        ViewBag.WelcomeName = user.Name ?? user.Username;
                        try
                        {
                            ViewBag.Identified = true;
                            return View(dao.ListThreadForUser(session.UserId, user.Email));
                        }
                        catch (SqlException ex) when (ex.Number == 208)
                        {
                            System.Diagnostics.Trace.TraceError("ChatMessage table is missing: " + ex);
                            ModelState.AddModelError("", "Tính năng tin nhắn chưa được cài đặt trên cơ sở dữ liệu. " +
                                "Hãy chạy SQL/2026-11-01_tao_bang_chatmessage.sql trên database DBNoiThat rồi thử lại.");
                            ViewBag.Identified = false;
                            return View(new List<ChatMessage>());
                        }
                    }
                }
            }

            // Chua dang nhap -> hien form tra cuu (dung cho doc OTP khi dang ky/lay mat khau)
            ViewBag.Identified = false;
            return View(new List<ChatMessage>());
        }

        // POST /Chat/Find - tra cuu thread bang Email + Phone (khach chua dang nhap)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Find(string email, string phone)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone))
            {
                ModelState.AddModelError("", "Vui lòng nhập đủ Email và Số điện thoại để tra cứu tin nhắn!");
                ViewBag.Identified = false;
                return View("Index", new List<ChatMessage>());
            }

            List<ChatMessage> msgs;
            try
            {
                msgs = dao.ListGuestThread(email, phone);
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                System.Diagnostics.Trace.TraceError("ChatMessage table is missing: " + ex);
                ModelState.AddModelError("", "Tính năng tin nhắn chưa được cài đặt trên cơ sở dữ liệu. " +
                    "Hãy chạy SQL/2026-11-01_tao_bang_chatmessage.sql trên database DBNoiThat rồi thử lại.");
                ViewBag.Identified = false;
                return View("Index", new List<ChatMessage>());
            }

            if (msgs.Count == 0)
            {
                // Co the khach nhap phone dang "090..." trong khi DB luu dang int ->
                // thu them kieu khop UserId de tim thread da gan tai khoan
                var user = FindUserByEmailPhone(email, phone);
                if (user != null)
                {
                    msgs = dao.ListThreadForUser(user.UserId, user.Email);
                    ViewBag.WelcomeName = user.Name ?? user.Username;
                }
            }

            ViewBag.Identified = true;
            ViewBag.ThreadEmail = email.Trim();
            ViewBag.GuestPhone = ChatMessageDao.NormalizePhone(phone);
            if (msgs.Count == 0)
            {
                ModelState.AddModelError("", "Không tìm thấy tin nhắn nào cho Email + Số điện thoại này.");
            }
            return View("Index", msgs);
        }

        // POST /Chat/Send - khach (da dang nhap) gui tin nhan cho he thong ho tro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Send(string content)
        {
            var session = (UserLogin)Session[Commoncontent.user_sesion];
            if (session == null)
            {
                return RedirectToAction("Login", "RegisterAndLogin");
            }
            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction("Index");
            }
            if (content.Length > ChatMessage.MAX_NVARCHAR)
            {
                content = content.Substring(0, ChatMessage.MAX_NVARCHAR);
            }

            try
            {
                using (var db = new DBNoiThat())
                {
                    var user = db.Users.Find(session.UserId);
                    dao.Insert(new ChatMessage
                    {
                        UserId = session.UserId,
                        Email = user != null ? user.Email : null,
                        Phone = user != null ? user.Phone.ToString() : null,
                        Sender = user != null ? (user.Name ?? user.Username) : session.Username,
                        Content = content,
                        IsSystem = false,
                        Status = true
                    });
                }
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                System.Diagnostics.Trace.TraceError("ChatMessage table is missing: " + ex);
                TempData["ChatError"] = "Tính năng tin nhắn chưa được cài đặt trên cơ sở dữ liệu. " +
                    "Hãy chạy SQL/2026-11-01_tao_bang_chatmessage.sql trên database DBNoiThat rồi thử lại.";
            }
            return RedirectToAction("Index");
        }

        private static User FindUserByEmailPhone(string email, string phone)
        {
            var dtb = ChatMessageDao.NormalizePhone(phone);
            if (dtb == null) return null;
            long phoneNum;
            if (!long.TryParse(dtb, out phoneNum)) return null;

            using (var db = new DBNoiThat())
            {
                var mail = email.Trim().ToLower();
                return db.Users.FirstOrDefault(u => u.Email.ToLower() == mail && u.Phone == phoneNum);
            }
        }
    }
}

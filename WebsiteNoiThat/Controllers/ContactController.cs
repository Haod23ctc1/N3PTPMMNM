using Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WebsiteNoiThat.Models;

namespace WebsiteNoiThat.Controllers
{
    public class ContactController : Controller
    {
        // GET: Contact
        DBNoiThat db= new DBNoiThat();
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Contacts()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contacts(EmailModel obj)
        {
            if (string.IsNullOrWhiteSpace(obj.EmailCC))
            {
                ViewBag.Error = "Vui lòng nhập địa chỉ email của bạn để chúng tôi có thể gửi phản hồi hỗ trợ.";
                return View(obj);
            }

            if (string.IsNullOrWhiteSpace(obj.EMailBody))
            {
                ViewBag.Error = "Vui lòng nhập nội dung cần hỗ trợ.";
                return View(obj);
            }

            try
            {
                var model = new Contact();
                string title = string.IsNullOrWhiteSpace(obj.EmailSubject) ? "Hỗ trợ khách hàng" : obj.EmailSubject.Trim();
                model.Content = $"[{title}] - {obj.EMailBody.Trim()}";
                model.Status = false; // Chưa xử lý (chờ hỗ trợ)
                model.EmailCC = obj.EmailCC.Trim();
                db.Contacts.Add(model);
                db.SaveChanges();

                ViewBag.Status = "Yêu cầu hỗ trợ của bạn đã được gửi thành công! Đội ngũ hỗ trợ sẽ phản hồi qua email của bạn trong thời gian sớm nhất.";
                ModelState.Clear();
                return View(new EmailModel());
            }
            catch (Exception)
            {
                ViewBag.Error = "Có lỗi xảy ra khi lưu yêu cầu hỗ trợ. Vui lòng thử lại sau.";
            }

            return View(obj);
        }
    }
}
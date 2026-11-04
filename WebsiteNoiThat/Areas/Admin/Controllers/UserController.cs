using Models.EF;
using System;
using System.Data.Entity.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Common;
using WebsiteNoiThat.Models;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class UserController : HomeController
    {
        DBNoiThat db = new DBNoiThat();
        public new ActionResult Index()
        {
            return View();
        }

        [HasCredential(RoleId = "VIEW_ADMIN")]
        public ActionResult Show()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            var models = db.Users.Where(n => n.GroupId != "USER").ToList();
            return View(models);
        }

        [HttpGet]
        [HasCredential(RoleId = "ADD_ADMIN")]
        public ActionResult Add()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            ViewBag.ListGroups = new SelectList(db.UserGroups.Where(a => a.GroupId != "USER").ToList(), "GroupId", "Name");
            return View();
        }

        [HttpPost]
        [HasCredential(RoleId = "ADD_ADMIN")]
        public ActionResult Add(User n)
        {
            ViewBag.ListGroups = new SelectList(db.UserGroups.Where(a => a.GroupId != "USER").ToList(), "GroupId", "Name");
            // Kiem tra do manh mat khau (issue #130)
            if (!PasswordPolicy.IsValid(n.Password))
            {
                return JavaScript("alert('" + PasswordPolicy.RuleMessage + "');");
            }
            var model = new User();
            model.Name = n.Name;
            model.Address = n.Address;
            model.Phone = n.Phone;
            model.Username = n.Username;
            model.Password = Encryptor.MD5Hash(n.Password);
            model.Email = n.Email;
            model.GroupId = n.GroupId;
            model.Status = n.Status;
            db.Users.Add(model);
            db.SaveChanges();
            return RedirectToAction("Show");
        }

        [HttpGet]
        [HasCredential(RoleId = "EDIT_ADMIN")]
        public ActionResult Edit(int UserId)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            ViewBag.ListGroups = new SelectList(db.UserGroups.Where(a => a.GroupId != "USER").ToList(), "GroupId", "Name");
            var models= db.Users.Where(a => a.UserId == UserId).First();
            if (models == null)
            {
                Response.StatusCode = 404;
                return RedirectToAction("Show");
            }
            return View(models);
        }

        [HttpPost]
        [HasCredential(RoleId = "EDIT_ADMIN")]
        public ActionResult Edit(User n)
        {
            ViewBag.ListGroups = new SelectList(db.UserGroups.Where(a => a.GroupId != "USER").ToList(), "GroupId", "Name");
            if (ModelState.IsValid)
            {
                var models = db.Users.Where(a => a.UserId == n.UserId).First();
                models.Name = n.Name;
                models.Username = n.Username;
                if (!string.IsNullOrWhiteSpace(n.Password))
                {
                    // Kiem tra do manh mat khau khi co doi (issue #130)
                    if (!PasswordPolicy.IsValid(n.Password))
                    {
                        return JavaScript("alert('" + PasswordPolicy.RuleMessage + "');");
                    }
                    models.Password = Encryptor.MD5Hash(n.Password);
                }
                models.GroupId = n.GroupId;
                models.Phone = n.Phone;
                models.Status = n.Status;
                models.Email = n.Email;
                models.Address = n.Address;
                db.SaveChanges();
                return RedirectToAction("Show");
            }
            else
            {
                return JavaScript("alert('Error');");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredential(RoleId = "DELETE_ADMIN")]
        public ActionResult Delete(int? UserId)
        {
            if (!UserId.HasValue)
            {
                TempData["Error"] = "Không tìm thấy tài khoản cần xóa.";
                return RedirectToAction("Show");
            }

            var session = Session[Commoncontent.user_sesion_admin] as UserLogin;
            if (session != null && session.UserId == UserId.Value)
            {
                TempData["Error"] = "Không thể xóa tài khoản quản trị đang đăng nhập.";
                return RedirectToAction("Show");
            }

            var model = db.Users.Find(UserId.Value);
            if (model == null)
            {
                TempData["Error"] = "Tài khoản không tồn tại hoặc đã được xóa.";
                return RedirectToAction("Show");
            }

            if (model.GroupId == CommonConstant.ADMIN_GROUP &&
                db.Users.Count(user => user.GroupId == CommonConstant.ADMIN_GROUP && user.Status) <= 1)
            {
                TempData["Error"] = "Không thể xóa quản trị viên ADMIN đang hoạt động cuối cùng.";
                return RedirectToAction("Show");
            }

            db.Users.Remove(model);
            try
            {
                db.SaveChanges();
                TempData["Success"] = "Đã xóa tài khoản.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Tài khoản đang được dữ liệu khác sử dụng nên không thể xóa hẳn. " +
                    "Hãy vào Sửa và tắt trạng thái tài khoản để vô hiệu hóa.";
            }
            return RedirectToAction("Show");
        }


        [HttpGet]
        [HasCredential(RoleId = "VIEW_ADMIN")]
        public ActionResult UserProfile()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            var model = db.Users.First(n => n.UserId == session.UserId);
            ViewBag.username = session.Username;

            return View(model);
        }

        [HttpPost]
        [HasCredential(RoleId = "EDIT_ADMIN")]
        public ActionResult UserProfile(User a)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            var user = db.Users.Find(session.UserId);
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            user.Name = a.Name;
            user.Phone = a.Phone;
            user.Address = a.Address;
            user.Email = a.Email;
            user.Username = a.Username;
            user.GroupId = a.GroupId;
            user.Status = a.Status;
            db.SaveChanges();
            return RedirectToAction("UserProfile");
        }

    }
}
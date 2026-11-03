using Models.EF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Common;
using WebsiteNoiThat.Models;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class CustomerController : HomeController
    {
        DBNoiThat db = new DBNoiThat();
        // GET: Admin/Customer

        [HasCredential(RoleId = "VIEW_USER")]
        public ActionResult Show()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;
            var user = (from a in db.Users
                          join b in db.Cards on a.UserId equals b.UserId
                          into g 
                          from d in g.DefaultIfEmpty()
                        select new UserModelView
                        {
                            UserId=a.UserId,
                            Name = a.Name,
                            Address = a.Address,
                            Phone = a.Phone,
                            Username = a.Username,
                            Password = a.Password,
                            Email = a.Email,

                            Status = a.Status,

                            GroupId = a.GroupId,
                            NumberCard = d.NumberCard,
                            Indentification=d.Identification

                        }).ToList();

            var model = user.Where(n => n.GroupId == "USER")
                .OrderBy(n => n.UserId)
                .ToList();

            return View(model);
        }

        [HttpGet]
        [HasCredential(RoleId = "ADD_USER")]
        public ActionResult Add()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;
            //ViewBag.ListGroups = new SelectList(db.UserGroups.Where(a => a.GroupId != "USER").ToList(), "GroupId", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult Add(UserModelView n)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            var model = new User();

            model.Name = n.Name;
            model.Address = n.Address;
            model.Phone = n.Phone;
            model.Username = n.Username;
            model.Password = Encryptor.MD5Hash(n.Password);
            model.Email = n.Email;
            model.GroupId = "USER";
            model.Status = n.Status;

            db.Users.Add(model);
            db.SaveChanges();
            var models = new Card();
            models.NumberCard = 0;
            models.UserNumber = 0;
            models.UserId = n.UserId;
            models.Identification = n.Indentification;
            db.Cards.Add(models);

            db.SaveChanges();
            return RedirectToAction("Show");
        }

        [HttpGet]
        [HasCredential(RoleId = "EDIT_USER")]
        public ActionResult Edit(int UserId)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;
            //ViewBag.ListGroups = new SelectList(db.UserGroups.Where(a => a.GroupId != "USER").ToList(), "GroupId", "Name");

            var user = (from a in db.Users
                         join b in db.Cards on a.UserId equals b.UserId
                         into g
                        from d in g.DefaultIfEmpty()
                        select new UserModelView
                         {
                             UserId = a.UserId,
                             Name = a.Name,
                             Address = a.Address,
                             Phone = a.Phone,
                             Username = a.Username,
                             Password = a.Password,
                             Email = a.Email,

                             Status = a.Status,

                             GroupId = a.GroupId,
                             NumberCard=d.NumberCard,
                            Indentification = d.Identification

                        }).ToList();
            var models =user.Where(a => a.UserId == UserId).First();
            if (models == null)
            {
                Response.StatusCode = 404;
                return RedirectToAction("Show");
            }
            return View(models);
        }

        [HttpPost]
        [HasCredential(RoleId = "EDIT_USER")]
        public ActionResult Edit(UserModelView n)
        {
            
            if (ModelState.IsValid)
            {
                //db.Entry(n).State = System.Data.Entity.EntityState.Modified;
                var model = db.Users.SingleOrDefault(a => a.UserId == n.UserId);
               
                model.Name = n.Name;
                model.Address = n.Address;
                model.Phone = n.Phone;
                model.Username = n.Username;
                if (!string.IsNullOrWhiteSpace(n.Password))
                {
                    model.Password = Encryptor.MD5Hash(n.Password);
                }
                model.Email = n.Email;
                model.GroupId = n.GroupId;

                model.Status = n.Status;
                db.SaveChanges();
                var models = db.Cards.Where(a => a.UserId == model.UserId).SingleOrDefault();
                if(models!=null)
                {
                    models.NumberCard = n.NumberCard;
                    models.Identification = n.Indentification;
                    db.SaveChanges();
                    return RedirectToAction("Show");
                }
                else
                {
                    return RedirectToAction("Show");
                }
            }
            else
            {
                return JavaScript("alert('Error');");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredential(RoleId = "DELETE_USER")]
        public ActionResult ToggleStatus(int? UserId)
        {
            if (!UserId.HasValue)
            {
                TempData["Error"] = "Không tìm thấy khách hàng cần cập nhật trạng thái.";
                return RedirectToAction("Show");
            }

            var model = db.Users.Find(UserId.Value);
            if (model == null || model.GroupId != "USER")
            {
                TempData["Error"] = "Khách hàng không tồn tại.";
                return RedirectToAction("Show");
            }

            model.Status = !model.Status;
            db.SaveChanges();
            TempData["Success"] = model.Status
                ? "Đã kích hoạt lại tài khoản khách hàng."
                : "Đã vô hiệu hóa tài khoản. Lịch sử đơn hàng và dữ liệu liên quan được giữ nguyên.";

            return RedirectToAction("Show");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasCredential(RoleId = "DELETE_USER")]
        public ActionResult Delete(int? UserId)
        {
            if (!UserId.HasValue)
            {
                TempData["Error"] = "Không tìm thấy khách hàng cần xóa.";
                return RedirectToAction("Show");
            }

            var user = db.Users.Find(UserId.Value);
            if (user == null || user.GroupId != "USER")
            {
                TempData["Error"] = "Khách hàng không tồn tại.";
                return RedirectToAction("Show");
            }

            var references = GetUserReferences(user.UserId);
            if (references.Count > 0)
            {
                TempData["Error"] = "Không thể xóa user ID " + user.UserId +
                    " vì còn dữ liệu liên quan: " + string.Join(", ", references) +
                    ". Dữ liệu được giữ nguyên; hãy dùng Vô hiệu hóa nếu muốn ngăn tài khoản đăng nhập.";
                return RedirectToAction("Show");
            }

            db.Users.Remove(user);
            try
            {
                db.SaveChanges();
                TempData["Success"] = "Đã xóa user ID " + user.UserId + ".";
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                TempData["Error"] = "Không thể xóa user ID " + user.UserId +
                    " vì dữ liệu liên quan vừa phát sinh hoặc đang bị ràng buộc. Không có dữ liệu liên quan nào bị xóa.";
            }

            return RedirectToAction("Show");
        }

        private List<string> GetUserReferences(int userId)
        {
            var references = new List<string>();
            var connection = db.Database.Connection;
            var closeConnection = connection.State != ConnectionState.Open;

            if (closeConnection)
            {
                connection.Open();
            }

            try
            {
                var columns = new List<Tuple<string, string, string>>();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
SELECT s.name, t.name, c.name
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.columns c ON c.object_id = t.object_id
JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE c.name IN (N'UserId', N'ReviewedByUserId')
  AND ty.name IN (N'tinyint', N'smallint', N'int', N'bigint')
  AND NOT (s.name = N'dbo' AND t.name = N'User')
ORDER BY s.name, t.name, c.name;";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            columns.Add(Tuple.Create(reader.GetString(0), reader.GetString(1), reader.GetString(2)));
                        }
                    }
                }

                foreach (var column in columns)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT_BIG(1) FROM " +
                            QuoteIdentifier(column.Item1) + "." + QuoteIdentifier(column.Item2) +
                            " WHERE " + QuoteIdentifier(column.Item3) + " = @UserId";
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = "@UserId";
                        parameter.DbType = DbType.Int32;
                        parameter.Value = userId;
                        command.Parameters.Add(parameter);

                        var count = Convert.ToInt64(command.ExecuteScalar());
                        if (count > 0)
                        {
                            references.Add(column.Item1 + "." + column.Item2 + "." + column.Item3 +
                                " (" + count + " dòng)");
                        }
                    }
                }
            }
            finally
            {
                if (closeConnection)
                {
                    connection.Close();
                }
            }

            return references;
        }

        private static string QuoteIdentifier(string identifier)
        {
            return "[" + identifier.Replace("]", "]]") + "]";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Models.DAO;
using Models.EF;
using System.Web.Script.Serialization;
using PagedList;
using PagedList.Mvc;
using System.IO;
using WebsiteNoiThat.Common;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class ProductCateController : HomeController
    {
        DBNoiThat db = new DBNoiThat();

        public new ActionResult Index()
        {
            return RedirectToAction("Show");
        }

        [HasCredential(RoleId = "VIEW_CATE")]
        public ActionResult Show()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            //Đổi mã danh mục cha thành tên để hiển thị
            var categories = db.Categories.ToList();
            var parentNames = new Dictionary<int, string>();
            foreach (var item in categories)
            {
                parentNames[item.CategoryId] = item.Name ?? string.Empty;
            }
            ViewBag.ParentNames = parentNames;

            return View(categories);
        }

        [HttpGet]
        [HasCredential(RoleId = "ADD_CATE")]
        public ActionResult Add()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;
            ViewBag.Parents = new SelectList(db.Categories, "CategoryId", "Name");
            return View();
        }

        [HttpPost]
        [HasCredential(RoleId = "ADD_CATE")]
        public ActionResult Add(Category n)
        {
            if (string.IsNullOrWhiteSpace(n.Name))
            {
                ModelState.AddModelError("NameError", "Tên danh mục không được để trống");
                return View(n);
            }

            var model = db.Categories.SingleOrDefault(a => a.CategoryId == n.CategoryId);
            if (model != null)
            {
                ModelState.AddModelError("CateError", "Mã danh mục đã tồn tại, vui lòng chọn mã khác");
                return View(n);
            }
            else
            {
                db.Categories.Add(n);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã thêm danh mục: " + n.Name;
                return RedirectToAction("Show");
            }

        }

        [HttpGet]
        [HasCredential(RoleId = "UPDATE_CATE")]
        public ActionResult Edit(int CategoryId)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            Category a = db.Categories.SingleOrDefault(n => n.CategoryId == CategoryId);
            //Danh mục cha trừ chính nó (không cho tự làm cha của mình)
            ViewBag.Parents = new SelectList(db.Categories.Where(c => c.CategoryId != CategoryId), "CategoryId", "Name");
            return View(a);

        }

        [HttpPost]
        [HasCredential(RoleId = "UPDATE_CATE")]
        public ActionResult Edit(Category n)
        {
            if (ModelState.IsValid)
            {
                db.Entry(n).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Show");
            }
            else
            {
                return JavaScript("alert('Error');");
            }
        }


        [HttpPost]
        [HasCredential(RoleId = "DELETE_CATE")]
        public ActionResult Delete(int CategoryId)
        {
            //Không cho xoá danh mục đang có sản phẩm (tránh lỗi FK)
            var hasProducts = db.Products.Any(p => p.CateId == CategoryId);
            if (hasProducts)
            {
                TempData["ErrorMessage"] = "Không thể xoá: danh mục này đang có sản phẩm. Hãy chuyển sản phẩm sang danh mục khác trước.";
                return RedirectToAction("Show");
            }

            //Không cho xoá danh mục đang là danh mục cha của danh mục khác
            var hasChildren = db.Categories.Any(c => c.ParId == CategoryId);
            if (hasChildren)
            {
                TempData["ErrorMessage"] = "Không thể xoá: danh mục này đang là danh mục cha của danh mục khác.";
                return RedirectToAction("Show");
            }

            var cate = db.Categories.Find(CategoryId);
            if (cate != null)
            {
                db.Categories.Remove(cate);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã xoá danh mục: " + cate.Name;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục cần xoá.";
            }
            return RedirectToAction("Show");
        }

    }
}

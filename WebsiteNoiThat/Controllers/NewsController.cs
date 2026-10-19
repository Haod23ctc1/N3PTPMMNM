using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Models.DAO;
using Models.EF;

namespace WebsiteNoiThat.Controllers
{
    public class NewsController : Controller
    {
        // GET: News
        DBNoiThat db = new DBNoiThat();
        public ActionResult NewsHot()
        {
            var model = new NewsDao().NewsHot();
            return PartialView(model);
        }

        // GET: /tin-tuc - danh sách tin tức có phân trang
        public ActionResult Index(int page = 1)
        {
            int pageSize = 6;
            var all = new NewsDao().ListAll();
            int totalPage = (int)Math.Ceiling(all.Count / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPage && totalPage > 0) page = totalPage;

            var model = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Page = page;
            ViewBag.Totalpage = totalPage;
            ViewBag.Maxpage = 5;
            ViewBag.First = 1;
            ViewBag.Last = totalPage;
            ViewBag.Prev = page - 1;
            ViewBag.Next = page + 1;

            return View(model);
        }

        public ActionResult Show(int NewsId)
        {
            var model = new NewsDao().GetById(NewsId);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

    }
}
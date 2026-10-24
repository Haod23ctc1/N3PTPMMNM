using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Models;
using Models.DAO;
using Models.EF;
using PagedList;

namespace WebsiteNoiThat.Controllers
{
    public class ProductController : Controller
    {
        DBNoiThat db  = new DBNoiThat();
        // GET: Product
        public ActionResult Index()
        {
            return View();
        }

        //cho home page
        public ActionResult ProductHot()
        {
            var model = new ProductDao().ProductHot();
            return PartialView(model);
        }
        public ActionResult SaleProduct()
        {
            var model = new ProductDao().SaleProduct();
            return PartialView(model);
        }
        public ActionResult CateHavana()
        {
            var model = new ProductDao().CateHavana();
            return PartialView(model);
        }
        public ActionResult CateKorea()
        {
            var model = new ProductDao().CateKorea();
            return PartialView(model);
        }
        public ActionResult NewProduct()
        {
            var model = new ProductDao().NewProduct();
            return PartialView(model);
        }
       
        public ActionResult DetailProduct(int Id)
        {
          var model = new ProductDao().DetailsProduct(Id);
            if (model == null)
            {
                Response.StatusCode = 404;
                Response.TrySkipIisCustomErrors = true;
                return View("NotFound");
            }
            var feedbackDao = new FeedbackDao();
            ViewBag.Feedbacks = feedbackDao.ListByProduct(Id);
            ViewBag.AvgRate = feedbackDao.AverageRate(Id);
            ViewBag.FeedbackCount = feedbackDao.CountByProduct(Id);
            return View(model);

        }

        // Gui danh gia san pham (issue #76): JS goi qua ajax, ngung hoc user phai dang nhap
        [HttpPost]
        public JsonResult AddFeedback(int productId, int rate, string content)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
            if (session == null)
                return Json(new { success = false, error = "login" });

            if (rate < 1 || rate > 5 || string.IsNullOrWhiteSpace(content) || content.Trim().Length > 500)
                return Json(new { success = false, error = "data" });

            try
            {
                using (var dao = new FeedbackDao())
                {
                    dao.Insert(new Feedback
                    {
                        ProductId = productId,
                        UserId = session.UserId,
                        Rate = rate,
                        Content = content.Trim(),
                        Time = DateTime.Now
                    });
                    return Json(new
                    {
                        success = true,
                        average = dao.AverageRate(productId),
                        count = dao.CountByProduct(productId)
                    });
                }
            }
            catch
            {
                return Json(new { success = false, error = "db" });
            }
        }

        //in ra theo danh mục sản phẩm
        public ActionResult CategoryShow(int cateId, int page = 1, int pagesize = 16)
        {
            var category = new CategoryDao().ViewDetail(cateId);
            ViewBag.CategoryShow = category;
            int total = 0;
            var model = new ProductDao().ListByCategoryId(cateId, ref total,page, pagesize);
            ViewBag.Total = total;
            ViewBag.Page = page;
            int maxpage = 10;
            int totalpage = 0;
           totalpage = (int)Math.Ceiling((double)total/pagesize);
            ViewBag.Totalpage = totalpage;
            ViewBag.Maxpage = maxpage;
            ViewBag.First = 1;
            ViewBag.Last = maxpage;
            ViewBag.Next = page + 1;
            ViewBag.Prev = page- 1;

            return View(model);
        }

        //tìm kiếm
        public ActionResult Search(string keyword, int page = 1, int pagesize = 16)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                // Không có từ khóa -> về trang chủ và báo lỗi
                TempData["SearchError"] = "Vui lòng nhập từ khóa để tìm kiếm.";
                return RedirectToAction("Index", "Home");
            }

            int total = 0;
            var model = new ProductDao().Search(keyword, ref total, page, pagesize);
            ViewBag.Total = total;
            ViewBag.Page = page;
            int maxpage = 10;
            int totalpage = 0;
            totalpage = (int)Math.Ceiling((double)total / pagesize);
            ViewBag.Totalpage = totalpage;
            ViewBag.Maxpage = maxpage;
            ViewBag.First = 1;
            ViewBag.Keyword = keyword;
            ViewBag.Last = maxpage;
            ViewBag.Next = page + 1;
            ViewBag.Prev = page - 1;

            return View(model);
        }
        
        //tìm theo khoảng giá (issue #73: cho phép tích nhiều mức giá cùng lúc, hiển thị sản phẩm của tất cả các mức đã tích)
        public ActionResult SearchFocus(bool? check0, bool? check1, bool? check2, bool? check3, bool? check4, int? page)
        {
            int pagenumber = (page ?? 1);
            int pagesize = 16;
            ViewBag.Check0 = check0 == true;
            ViewBag.Check1 = check1 == true;
            ViewBag.Check2 = check2 == true;
            ViewBag.Check3 = check3 == true;
            ViewBag.Check4 = check4 == true;

            // Gom tất cả các mức giá người dùng đã tích: (giá trị nhỏ nhất, giá trị lớn nhất hoặc null nếu không chặn trên)
            var ranges = new List<Tuple<int, int?>>();
            if (check0 == true) ranges.Add(Tuple.Create(0, (int?)5000000));
            if (check1 == true) ranges.Add(Tuple.Create(5000000, (int?)10000000));
            if (check2 == true) ranges.Add(Tuple.Create(10000000, (int?)20000000));
            if (check3 == true) ranges.Add(Tuple.Create(20000000, (int?)50000000));
            if (check4 == true) ranges.Add(Tuple.Create(50000000, (int?)null));
            if (ranges.Count == 0)
            {
                return View();
            }

            // Ghép các khoảng bằng điều kiện HOẶC để lấy gộp sản phẩm của mọi khoảng đã tích
            var param = Expression.Parameter(typeof(Product), "n");
            Expression body = null;
            foreach (var r in ranges)
            {
                var price = Expression.Property(param, nameof(Product.Price));
                Expression part = Expression.GreaterThan(price, Expression.Constant(r.Item1, typeof(int?)));
                if (r.Item2.HasValue)
                {
                    part = Expression.AndAlso(part, Expression.LessThanOrEqual(price, Expression.Constant(r.Item2.Value, typeof(int?))));
                }
                body = body == null ? part : Expression.OrElse(body, part);
            }
            var lambda = Expression.Lambda<Func<Product, bool>>(body, param);

            var model = db.Products.Where(lambda).OrderBy(n => n.Price).ToPagedList(pagenumber, pagesize);
            return View(model);
        }

        public JsonResult ListName(string q)
        {
            var data = new ProductDao().ListName(q);
            return Json(new
            {
                data = data,
                status = true
            },JsonRequestBehavior.AllowGet);
        }

        // Xem tất cả sản phẩm
         public ActionResult ShowProduct(int? page)
        {
            int pagenumber = (page ?? 1);
            int pagesize = 16;
            var model =db.Products.Where(n => n.Discount == 0 || n.EndDate < DateTime.Now || n.StartDate > DateTime.Now).OrderBy(n=>n.Name).ToPagedList(pagenumber, pagesize);
            return View(model);
        }

        // Xem tất cả sản phẩm hot
        public ActionResult Hots(int? page)
        {
            int pagenumber = (page ?? 1);
            int pagesize = 16;
            var model = (from a in db.Products
                         join b in db.OrderDetails on a.ProductId equals b.ProductId
                         group b by new { a.Description, a.ProductId, a.Photo, a.Price, a.Discount, a.EndDate, a.StartDate }
                         into g
                         select new ProductView
                         {
                             Description = g.Key.Description,
                             ProductId = g.Key.ProductId,
                             Price = g.Key.Price,
                             Discount = g.Key.Discount,
                             StartDate = g.Key.StartDate,
                             EndDate = g.Key.EndDate,
                             Photo = g.Key.Photo,
                             Quantity = g.Sum(s => s.Quantity),

                         }).OrderByDescending(n => n.Quantity).ToPagedList(pagenumber, pagesize);
            return View(model);
        }

        // Xem tất cả sản phẩm sale
        public ActionResult Sales(int? page)
        {
            int pagenumber = (page ?? 1);
            int pagesize = 16;
            var model = db.Products.Where(n=>n.Discount>0).OrderBy(n => n.Name).ToPagedList(pagenumber, pagesize);
            //var model = db.Products.Where(n=>n.Discount>0 && n.EndDate > DateTime.Now && n.StartDate < DateTime.Now).OrderBy(n => n.Name).ToPagedList(pagenumber, pagesize);
            return View(model);
        }
      

    }
}
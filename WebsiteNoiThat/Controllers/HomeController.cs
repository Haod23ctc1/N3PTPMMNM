using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Common;
using WebsiteNoiThat.Models;

namespace WebsiteNoiThat.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        [ChildActionOnly]
        public PartialViewResult HeaderCart()
        {
            var cart = Session[Commoncontent.CartSession];
            var list = new List<CartItem>();
            double total = 0;
            if (cart != null)
            {
                list = (List<CartItem>)cart;
                foreach (var item in list)
                {
                    total += Convert.ToDouble(item.Product.Price * item.Quantity - item.Product.Price * item.Product.Discount * 0.01 * item.Quantity);
                }
            }
            ViewBag.Total = total;

            return PartialView(list);
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Models.EF;
using WebsiteNoiThat.Areas.Admin.Models;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        // GET: Admin/Home
        public virtual ActionResult Index()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            if (session != null)
            {
                ViewBag.username = session.Username;

                try
                {
                    return View(BuildDashboard());
                }
                catch (Exception ex)
                {
                    // Không để trang tổng quan "chết" nếu dữ liệu/cấu hình có vấn đề.
                    ViewBag.DashboardError = "Không thể tải số liệu thống kê: " + ex.Message;
                    return View(new DashboardViewModel());
                }
            }
            else
            {
                return Redirect("~/Admin/Login");
            }
        }

        /// <summary>
        /// Tổng hợp số liệu thống kê cho trang tổng quan quản trị (issue #155).
        /// </summary>
        private DashboardViewModel BuildDashboard()
        {
            using (var db = new DBNoiThat())
            {
                var model = new DashboardViewModel();

                var statuses = db.Status.ToList();
                var orders = db.Orders.ToList();
                var orderDetails = db.OrderDetails.ToList();

                // Các trạng thái được coi là đã thanh toán (theo tên, dự phòng StatusId = 5).
                var paidStatusIds = statuses
                    .Where(s => !string.IsNullOrEmpty(s.Name)
                                && s.Name.IndexOf("thanh toán", StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(s => s.StatusId)
                    .ToList();
                if (paidStatusIds.Count == 0)
                {
                    paidStatusIds.Add(5);
                }

                model.ProductCount = db.Products.Count();
                model.OrderCount = orders.Count;
                model.CustomerCount = db.Users.Count(u => u.GroupId == "USER");
                model.PendingOrderCount = orders.Count(o => o.StatusId == 1 || o.StatusId == 2);
                model.PendingContactCount = db.Contacts.Count(c => c.Status != true);

                // Doanh thu: tổng tiền hàng của các đơn đã thanh toán.
                var paidOrderIds = new HashSet<int>(
                    orders.Where(o => paidStatusIds.Contains(o.StatusId)).Select(o => o.OrderId));
                model.Revenue = orderDetails
                    .Where(d => d.OrderId.HasValue && paidOrderIds.Contains(d.OrderId.Value))
                    .Sum(d => (decimal)(d.Price ?? 0) * (d.Quantity ?? 0));

                // Đơn hàng theo trạng thái.
                var statusNames = statuses.ToDictionary(s => s.StatusId, s => s.Name);
                model.OrdersByStatus = orders
                    .GroupBy(o => o.StatusId)
                    .OrderBy(g => g.Key)
                    .Select(g => new DashboardStatusItem
                    {
                        Name = statusNames.ContainsKey(g.Key) && !string.IsNullOrEmpty(statusNames[g.Key])
                            ? statusNames[g.Key]
                            : "Trạng thái #" + g.Key,
                        Count = g.Count()
                    })
                    .ToList();

                // Top 5 sản phẩm bán chạy theo số lượng đã bán.
                var productNames = db.Products
                    .Select(p => new { p.ProductId, p.Name })
                    .ToDictionary(p => p.ProductId, p => p.Name);
                model.TopProducts = orderDetails
                    .Where(d => d.ProductId.HasValue && productNames.ContainsKey(d.ProductId.Value))
                    .GroupBy(d => d.ProductId.Value)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        Quantity = g.Sum(d => d.Quantity ?? 0)
                    })
                    .OrderByDescending(x => x.Quantity)
                    .Take(5)
                    .Select(x => new DashboardProductItem
                    {
                        Name = productNames.ContainsKey(x.ProductId) && !string.IsNullOrEmpty(productNames[x.ProductId])
                            ? productNames[x.ProductId]
                            : "Sản phẩm #" + x.ProductId,
                        Quantity = x.Quantity
                    })
                    .ToList();

                // 5 đơn hàng gần đây nhất kèm tổng tiền.
                var totals = orderDetails
                    .Where(d => d.OrderId.HasValue)
                    .GroupBy(d => d.OrderId.Value)
                    .ToDictionary(g => g.Key, g => g.Sum(d => (decimal)(d.Price ?? 0) * (d.Quantity ?? 0)));
                model.RecentOrders = orders
                    .OrderByDescending(o => o.UpdateDate)
                    .ThenByDescending(o => o.OrderId)
                    .Take(5)
                    .Select(o => new DashboardOrderItem
                    {
                        OrderId = o.OrderId,
                        ShipName = o.ShipName,
                        UpdateDate = o.UpdateDate,
                        StatusId = o.StatusId,
                        StatusName = statusNames.ContainsKey(o.StatusId) && !string.IsNullOrEmpty(statusNames[o.StatusId])
                            ? statusNames[o.StatusId]
                            : "Trạng thái #" + o.StatusId,
                        Total = totals.ContainsKey(o.OrderId) ? totals[o.OrderId] : 0m
                    })
                    .ToList();

                return model;
            }
        }
    }
}

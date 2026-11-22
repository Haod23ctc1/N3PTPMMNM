using System;
using System.Collections.Generic;

namespace WebsiteNoiThat.Areas.Admin.Models
{
    /// <summary>
    /// Dữ liệu tổng hợp cho trang tổng quan (dashboard) của khu vực quản trị.
    /// </summary>
    public class DashboardViewModel
    {
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }
        public int CustomerCount { get; set; }
        public int PendingOrderCount { get; set; }
        public int PendingContactCount { get; set; }

        /// <summary>Tổng doanh thu của các đơn đã thanh toán.</summary>
        public decimal Revenue { get; set; }

        public List<DashboardStatusItem> OrdersByStatus { get; set; }
        public List<DashboardProductItem> TopProducts { get; set; }
        public List<DashboardOrderItem> RecentOrders { get; set; }

        public DashboardViewModel()
        {
            OrdersByStatus = new List<DashboardStatusItem>();
            TopProducts = new List<DashboardProductItem>();
            RecentOrders = new List<DashboardOrderItem>();
        }
    }

    public class DashboardStatusItem
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public class DashboardProductItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }

    public class DashboardOrderItem
    {
        public int OrderId { get; set; }
        public string ShipName { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public decimal Total { get; set; }
    }
}

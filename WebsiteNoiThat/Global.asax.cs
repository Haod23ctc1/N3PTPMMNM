using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Models.EF;

namespace WebsiteNoiThat
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Tự động kiểm tra và cập nhật database schema
            AutoUpdateDatabase();
        }

        private void AutoUpdateDatabase()
        {
            try
            {
                using (var db = new DBNoiThat())
                {
                    // Tự động bổ sung cột PaymentMethod nếu thiếu (Issue #67)
                    var checkSql = "IF COL_LENGTH('[Order]', 'PaymentMethod') IS NULL " +
                                   "ALTER TABLE [Order] ADD PaymentMethod NVARCHAR(50) NULL";
                    db.Database.ExecuteSqlCommand(checkSql);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Database auto-update failed: " + ex.Message);
            }
        }

        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {
            if (Request == null || Response == null || !Request.IsSecureConnection || Request.IsLocal)
            {
                return;
            }

            Response.AddHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        }
    }
}

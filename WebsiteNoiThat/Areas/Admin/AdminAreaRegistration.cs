using System.Web.Mvc;

namespace WebsiteNoiThat.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new
                {
                    controller = "Home",
                    action = "Index",
                    id = UrlParameter.Optional
                },
                new[]
                {
                    "WebsiteNoiThat.Areas.Admin.Controllers"
                }
            );

            context.MapRoute(
                "SearchOrder",
                "Admin/Search/{d}",
                new
                {
                    controller = "Order",
                    action = "Search",
                    d = UrlParameter.Optional
                },
                new[]
                {
                    "WebsiteNoiThat.Areas.Admin.Controllers"
                }
            );
        }
    }
}
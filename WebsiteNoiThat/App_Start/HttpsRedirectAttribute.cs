using System;
using System.Configuration;
using System.Web.Mvc;

namespace WebsiteNoiThat
{
    /// <summary>
    /// Ép toàn bộ request dùng HTTPS. Khác RequireHttpsAttribute mặc định:
    /// hỗ trợ cổng SSL riêng của IIS Express (HTTP và HTTPS không cùng port).
    /// </summary>
    public class HttpsRedirectAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            var request = filterContext.HttpContext.Request;
            // IIS Express trong Cursor chỉ bind HTTP (vd. :8080), không có cổng SSL.
            // Ép HTTPS trên localhost sẽ redirect tới cổng trống → ERR_CONNECTION_REFUSED.
            if (request.IsSecureConnection || request.IsLocal)
            {
                return;
            }

            if (!string.Equals(request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(request.HttpMethod, "HEAD", StringComparison.OrdinalIgnoreCase))
            {
                filterContext.Result = new HttpStatusCodeResult(403, "Yeu cau ket noi HTTPS.");
                return;
            }

            int httpsPort = ResolveHttpsPort();
            string host = request.Url.Host;
            string pathAndQuery = request.Url.PathAndQuery;
            string url = httpsPort == 443
                ? "https://" + host + pathAndQuery
                : "https://" + host + ":" + httpsPort + pathAndQuery;

            filterContext.Result = new RedirectResult(url, true);
        }

        private static int ResolveHttpsPort()
        {
            int port;
            if (int.TryParse(ConfigurationManager.AppSettings["HttpsPort"], out port) && port > 0)
            {
                return port;
            }

#if DEBUG
            return 44373;
#else
            return 443;
#endif
        }
    }
}

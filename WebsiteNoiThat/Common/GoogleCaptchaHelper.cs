using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Net;

namespace WebsiteNoiThat.Common
{
    public static class GoogleCaptchaHelper
    {
        public static bool Verify(string responseToken)
        {
            if (string.IsNullOrEmpty(responseToken))
            {
                return false;
            }

            string secret = ConfigurationManager.AppSettings["GoogleReCaptchaSecretKey"];
            using (var client = new WebClient())
            {
                string url = $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={responseToken}";
                string result = client.DownloadString(url);
                var json = JObject.Parse(result);
                return (bool)json["success"];
            }
        }
    }
}
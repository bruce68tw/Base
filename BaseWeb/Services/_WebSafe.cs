using Newtonsoft.Json.Linq;
using System.Web;
//.Security.AntiXss;

namespace BaseWeb.Services
{
    /// <summary>
    /// handle web security issue
    /// </summary>
    public class _WebSafe
    {
        public static string JsonToStr(JObject json)
        {
            return (json == null)
                ? ""
                : HttpUtility.HtmlEncode(json.ToString());
        }

    }//class
}

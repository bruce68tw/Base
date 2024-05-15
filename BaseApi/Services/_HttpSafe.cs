using Newtonsoft.Json.Linq;
using System.Web;
//.Security.AntiXss;

namespace BaseApi.Services
{
    /// <summary>
    /// handle web security issue
    /// </summary>
    public class _HttpSafe
    {
        public static string JsonToStr(JObject json)
        {
            return (json == null)
                ? ""
                : HttpUtility.HtmlEncode(json.ToString());
        }

    }//class
}

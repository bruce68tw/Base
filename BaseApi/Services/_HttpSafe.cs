using Newtonsoft.Json.Linq;
using System.Web;
//.Security.AntiXss;

namespace BaseApi.Services
{
    /// <summary>
    /// 將 JSON 物件轉成經 HTML 編碼的字串。
    /// </summary>
    public class _HttpSafe
    {
        /// <summary>
        /// 序列化 JSON 後進行 HTML 字元編碼。輸入為 null 時回傳空字串。
        /// </summary>
        /// <param name="json">要序列化並做 HTML 編碼的 JSON 物件，null 會回傳空字串。</param>
        public static string JsonToStr(JObject json)
        {
            return (json == null)
                ? ""
                : HttpUtility.HtmlEncode(json.ToString());
        }

    }//class
}

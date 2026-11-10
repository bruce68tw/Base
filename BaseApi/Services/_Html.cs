using Base.Services;
using System.Text.RegularExpressions;
using System.Web;

namespace BaseApi.Services
{
    /// <summary>
    /// 提供 HTML 字元編碼、解碼與標籤移除工具。
    /// </summary>
    public static class _Html
    {
        /// <summary>
        /// 將文字中的特殊字元轉換為 HTML 字元實體。
        /// </summary>
        /// <param name="data">要轉換成 HTML 字元實體的文字。</param>
        public static string Encode(string data)
        {
            return HttpUtility.HtmlEncode(data);
        }

        /// <summary>
        /// 將 HTML 字元實體解碼為文字。
        /// </summary>
        /// <param name="value">包含待解碼 HTML 字元實體的文字。</param>
        public static string Decode(string value)
        {
            return HttpUtility.HtmlDecode(value);
        }

        /// <summary>
        /// 移除 HTML 標籤。
        /// 同時移除符合比對規則的 script 區塊。
        /// </summary>
        /// <param name="html">要移除 script 區塊與標籤的 HTML 字串。</param>
        public static string RemoveHtmlTag(string html)
        {
            if (_Str.NotEmpty(html))
            {
                //remove js code.
                html = Regex.Replace(html, @"<script[\d\D]*?>[\d\D]*?</script>", string.Empty);

                //remove html tag.
                html = Regex.Replace(html, @"<[^>]*>", string.Empty);
            }            

            return html;
        }

    } //class
}

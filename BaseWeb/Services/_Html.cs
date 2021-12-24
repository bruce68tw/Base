using Base.Services;
using System.Text.RegularExpressions;
using System.Web;

namespace BaseWeb.Services
{
    public static class _Html
    {
        /// <summary>
        /// convert value to Html encoding for special code
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Encode(string data)
        {
            return HttpUtility.HtmlEncode(data);
        }

        public static string Decode(string value)
        {
            return HttpUtility.HtmlDecode(value);
        }

        /// <summary>
        /// remove html tag
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string RemoveHtmlTag(string html)
        {
            if (!_Str.IsEmpty(html))
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

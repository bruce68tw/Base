using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>呈現作業路徑文字的 MVC ViewComponent。</summary>
    public class XgProgPathViewComponent : ViewComponent
    {
        /// <summary>產生作業路徑文字的 HTML。</summary>
        /// <param name="path">要顯示的作業路徑。</param>
        /// <returns></returns>
        public HtmlString Invoke(string path = "")
        {
            var html = $"<div class='x-prog-path'>{path}</div>";
            return new HtmlString(html);
        }

    } //class
}
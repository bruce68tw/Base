using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>呈現共用樣式的水平分隔線的 MVC ViewComponent。</summary>
    public class XgHrViewComponent : ViewComponent
    {
        /// <summary>產生共用樣式的水平分隔線的 HTML。</summary>
        public HtmlString Invoke()
        {
            return new HtmlString("<hr class='x-hr'>");
        }

    }//class
}

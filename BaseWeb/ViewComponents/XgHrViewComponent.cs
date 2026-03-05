using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XgHrViewComponent : ViewComponent
    {
        /// <summary>
        /// 水平分隔線
        /// </summary>
        public HtmlString Invoke()
        {
            return new HtmlString("<hr class='x-hr'>");
        }

    }//class
}

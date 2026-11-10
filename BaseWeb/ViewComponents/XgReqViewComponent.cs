using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //require 星號
    /// <summary>呈現必填星號的 MVC ViewComponent。</summary>
    public class XgReqViewComponent : ViewComponent
    {
        /// <summary>產生必填星號的 HTML。</summary>
        public HtmlString Invoke()
        {
            return new HtmlString("<span class='x-required'>*</span>");
        }

    } //class
}
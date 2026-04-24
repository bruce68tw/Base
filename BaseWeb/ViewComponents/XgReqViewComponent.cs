using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //require 星號
    public class XgReqViewComponent : ViewComponent
    {
        public HtmlString Invoke()
        {
            return new HtmlString("<span class='x-required'>*</span>");
        }

    } //class
}
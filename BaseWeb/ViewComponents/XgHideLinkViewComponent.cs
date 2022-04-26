using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //hidden link for pjax call
    public class XgHideLinkViewComponent : ViewComponent
    {
        public HtmlString Invoke()
        {
            var html = "<a id='hideLink' data-pjax style='display:none'></a>";
            return new HtmlString(html);
        }
    }//class
}

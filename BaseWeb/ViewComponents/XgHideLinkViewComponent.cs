using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //hidden link for pjax call
    /// <summary>呈現供程式觸發的隱藏 PJAX 連結的 MVC ViewComponent。</summary>
    public class XgHideLinkViewComponent : ViewComponent
    {
        /// <summary>產生供程式觸發的隱藏 PJAX 連結的 HTML。</summary>
        public HtmlString Invoke()
        {
            var html = "<a id='hideLink' data-pjax class='d-none'></a>";
            return new HtmlString(html);
        }
    }//class
}

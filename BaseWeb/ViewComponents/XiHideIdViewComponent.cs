using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>呈現主鍵隱藏欄位的 MVC ViewComponent。</summary>
    public class XiHideIdViewComponent : ViewComponent
    {
        /// <summary>產生主鍵隱藏欄位的 HTML。</summary>
        public HtmlString Invoke()
        {
            return new HtmlString(_Input.XiHideId());
        }

    }//class
}

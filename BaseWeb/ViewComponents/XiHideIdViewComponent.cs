using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XiHideIdViewComponent : ViewComponent
    {
        /// <summary>
        /// hidden field
        /// </summary>
        public HtmlString Invoke()
        {
            return new HtmlString(_Input.XiHideId());
        }

    }//class
}

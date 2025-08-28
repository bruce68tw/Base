using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XiHideViewComponent : ViewComponent
    {
        /// <summary>
        /// hidden field
        /// </summary>
        public HtmlString Invoke(XiHideDto dto)
        {
            return new HtmlString(_Input.XiHide(dto));
        }

    }//class
}

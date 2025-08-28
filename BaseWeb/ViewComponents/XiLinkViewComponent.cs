using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>
    /// link file
    /// </summary>
    public class XiLinkViewComponent : ViewComponent
    {
        public HtmlString Invoke(XiLinkDto dto)
        {
            return new HtmlString(_Input.XiLink(dto));
        }
        
    } //class
}

using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>
    /// html editor
    /// </summary>
    public class XiHtmlViewComponent : ViewComponent
    {
        public HtmlString Invoke(XiHtmlDto dto)
        {
            return new HtmlString(_Input.XiHtml(dto));
        }

    } //class
}
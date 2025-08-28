using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //textarea input
    public class XiTextareaViewComponent : ViewComponent
    {
        public HtmlString Invoke(XiTextareaDto dto)
        {
            return new HtmlString(_Input.XiTextarea(dto));
        }

    } //class
}
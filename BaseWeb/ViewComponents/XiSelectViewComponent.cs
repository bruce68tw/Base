using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //select input
    public class XiSelectViewComponent : ViewComponent
    {
        public HtmlString Invoke(XiSelectDto dto)
        {
            return new HtmlString(_Input.XiSelect(dto));
        } 

    }//class
}

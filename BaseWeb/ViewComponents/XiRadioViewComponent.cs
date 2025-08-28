using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

//TODO: modifing
namespace BaseWeb.ViewComponents
{
    //Radio button group, consider horizontal or vertical
    public class XiRadioViewComponent : ViewComponent
    {
        public HtmlString Invoke(XiRadioDto dto)
        {
            return new HtmlString(_Input.XiRadio(dto));
        }

    }//class
}

using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //integer/decimal input(3 different: class name, input arg, digit=true !!)
    public class XiDecViewComponent : ViewComponent
    {
        public HtmlString Invoke(XiDecDto dto)
        {
            return new HtmlString(_Input.XiDec(dto));
        } 

    }//class
}

using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //integer/decimal input(3 different: class name, input arg, digit=true !!)
    public class XiIntViewComponent : ViewComponent
    {
        public HtmlString Invoke(XiIntDto dto)
        {
            return new HtmlString(_Input.XiInt(dto));
        } 

    }//class
}

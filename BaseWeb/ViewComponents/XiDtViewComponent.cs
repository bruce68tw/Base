using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.Helpers
{
    //use bootstrap datepicker
    public class XiDtViewComponent : ViewComponent
    {
        public HtmlString Invoke(XiDtDto dto)
        {
            return new HtmlString(_Input.XiDt(dto));
        } 

    }//calss
}

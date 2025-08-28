using Base.Services;
using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.Helpers
{
    //use bootstrap datepicker
    public class XiDateViewComponent : ViewComponent
    {
        /// <summary>
        /// date field
        /// </summary>
        /// <returns></returns>
        public HtmlString Invoke(XiDateDto dto)
        {
            return new HtmlString(_Input.XiDate(dto));
        } 

    }//calss
}

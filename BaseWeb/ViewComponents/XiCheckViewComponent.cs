using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>
    /// checkbox component
    /// </summary>
    public class XiCheckViewComponent : ViewComponent
    {
        /// <summary>
        /// checkbox component
        /// </summary>
        /// <returns></returns>
        public HtmlString Invoke(XiCheckDto dto)
        {
            return new HtmlString(_Input.XiCheck(dto));
        }

    }//class
}
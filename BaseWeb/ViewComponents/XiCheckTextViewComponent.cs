using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>
    /// checkbox component
    /// </summary>
    public class XiCheckTextViewComponent : ViewComponent
    {
        /// <summary>
        /// old args: string fid, string label, int len, string cls
        /// checkbox component
        /// </summary>
        /// <returns></returns>
        public HtmlString Invoke(XiCheckTextDto dto)
        {
            return new HtmlString(_Input.XiCheckText(dto));
        }

    }//class
}
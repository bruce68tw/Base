using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>
    /// radio with text component
    /// </summary>
    public class XiRadioTextViewComponent : ViewComponent
    {
        /// <summary>
        /// old args: string fid, string label, int len, string cls
        /// checkbox component
        /// </summary>
        /// <returns></returns>
        public HtmlString Invoke(XiRadioTextDto dto)
        {
            return new HtmlString(_Input.XiRadioText(dto));
        }

    }//class
}
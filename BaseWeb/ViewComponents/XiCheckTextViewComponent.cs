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
        /// checkbox component
        /// </summary>
        /// <returns></returns>
        public HtmlString Invoke(string fid, string label, int len, string cls)
        {
            return new HtmlString(_Input.XiCheckText(fid, label, len, cls));
        }

    }//class
}
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XiHideViewComponent : ViewComponent
    {
        /// <summary>
        /// hidden field
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HtmlString Invoke(string fid, string value = "", string extAttr = "", string extClass = "")
        {
            var attr = _Helper.GetInputAttr(fid);
            if (!string.IsNullOrEmpty(extClass))
                attr += $" class='{extClass}'";
            var html = $"<input{attr} data-type='text' type='hidden' value='{value}' {extAttr}>";
            return new HtmlString(html);
        }

    }//class
}

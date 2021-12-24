using Base.Services;
using BaseWeb.Models;
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
        public HtmlString Invoke(XiHideDto dto)
        {
            var attr = _Helper.GetInputAttr(dto.Fid, "", false, dto.InputAttr);
            if (!_Str.IsEmpty(dto.BoxClass))
                attr += $" class='{dto.BoxClass}'";

            var html = $"<input{attr} data-type='text' type='hidden' value='{dto.Value}'>";
            return new HtmlString(html);
        }

    }//class
}

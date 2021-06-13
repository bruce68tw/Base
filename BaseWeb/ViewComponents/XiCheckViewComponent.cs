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
            //prop ??= new PropCheckDto();
            //if (label == "")
            //    label = "&nbsp;";   //add space, or position will wrong

            //get attr
            var attr = _Helper.GetInputAttr(dto.Fid, dto.Edit);
            if (dto.IsCheck)
                attr += " checked";
            if (dto.FnOnClick != "")
                attr += $" onclick='{dto.FnOnClick}'";

            //ext class
            if (dto.Label == "")
                dto.ExtClass += " xg-no-label";

            //get html (span for checkbox checked sign)
            //value attr will disappear, use data-value instead !!
            var html = $@"
<label class='xi-check {dto.ExtClass}' {dto.ExtAttr}>
    <input{attr} type='checkbox' data-type='check' data-value='{dto.Value}'>{dto.Label}
    <span class='xi-cspan'></span>
</label>";

            //add title
            if (!string.IsNullOrEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, false, dto.LabelTip, dto.InRow, dto.Cols);

            return new HtmlString(html);
        }

    }//class
}
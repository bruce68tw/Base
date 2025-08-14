using Base.Services;
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

            //set default 
            dto.Value = _Str.EmptyToValue(dto.Value, "1");

            //get attr
            var attr = _Helper.GetInputAttr(dto.Fid, dto.Edit, false, dto.InputAttr);
            if (dto.IsCheck)
                attr += " checked";
            if (_Str.NotEmpty(dto.FnOnClick))
                attr += $" data-onclick='{dto.FnOnClick}'";

            //ext class
            if (string.IsNullOrEmpty(dto.Label))
                dto.BoxClass += " x-no-label";

            //get html (span for checkbox checked sign)
            //value attr will disappear, use data-value instead !!
            var css = _Helper.GetCssClass("xi-check", dto.BoxClass, dto.Width);
            var html = $@"
<label class='{css}'>
    <input{attr} type='checkbox' data-type='check' data-value='{dto.Value}'>{dto.Label}
    <span class='xi-cspan'></span>
</label>";

            //add title
            if (_Str.NotEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, false, dto.LabelTip, dto.InRow, dto.Cols, true);

            return new HtmlString(html);
        }

    }//class
}
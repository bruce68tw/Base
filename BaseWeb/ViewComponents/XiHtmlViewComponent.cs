using Base.Services;
using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>
    /// html editor
    /// </summary>
    public class XiHtmlViewComponent : ViewComponent
    {
        public HtmlString Invoke(XiHtmlDto dto)
        {
            var attr = _Helper.GetInputAttr(dto.Fid, dto.Edit, dto.Required, dto.InputAttr) +
                $" value='{dto.Value}'" +
                _Helper.GetPlaceHolder(dto.InputTip) +
                //_Helper.GetRequired(dto.Required) +
                _Helper.GetMaxLength(dto.MaxLen);

            //summernote will add div below textarea, so add div outside for validate msg
            var css = _Helper.GetCssClass("form-control xd-valid", dto.BoxClass, dto.Width);
            var html = $@"
<div class='xi-box'>
    <textarea{attr} data-type='html' class='{css}'></textarea>
</div>";
            if (_Str.NotEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, dto.Required, dto.LabelTip, dto.InRow, dto.Cols);
            return new HtmlString(html);
        }

    } //class
}
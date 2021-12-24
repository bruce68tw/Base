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
            /*
            string title, string fid, string value = "",
            int maxLen = 0, int rowsCount = 10,
            bool required = false, string edit = "", bool inRow = false,
            string labelTip = "", string inputTip = "",
            string extAttr = "", string extClass = "", string cols = ""
            
            var html = _Helper.GetTextareaHtml(title, fid, "html", value,
                maxLen, rowsCount,
                required, editable, inRow,
                labelTip, inputTip,
                extAttr, extClass, cols);
            return new HtmlString(html);
            */
            //rows='{dto.RowsCount}' 
            var attr = _Helper.GetInputAttr(dto.Fid, dto.Edit, dto.Required, dto.InputAttr) +
                $" value='{dto.Value}' style='width:{dto.Width}'" +
                _Helper.GetPlaceHolder(dto.InputTip) +
                _Helper.GetRequired(dto.Required) +
                _Helper.GetMaxLength(dto.MaxLen);
            //if (!_Str.IsEmpty(extAttr))
            //    attr += " " + extAttr;

            //html
            //summernote will add div below textarea, so add div outside for validate msg
            var html = $@"
<div class='xi-box {dto.BoxClass}'>
    <textarea{attr} data-type='html' class='form-control'></textarea>
</div>";
            if (!_Str.IsEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, dto.Required, dto.LabelTip, dto.InRow, dto.Cols);
            return new HtmlString(html);
        }

    } //class
}
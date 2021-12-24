using Base.Services;
using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //textarea input
    public class XiTextareaViewComponent : ViewComponent
    {
        public HtmlString Invoke(XiTextareaDto dto)
        {
            /*
             string title, string fid, string value = "",
            int maxLen = 0, int rowsCount = 3,
            bool required = false, string edit = "", bool inRow = false,
            string labelTip = "", string inputTip = "",
            string extClass = "", string extAttr = "", string cols = ""
             */
            //attr
            var attr = _Helper.GetInputAttr(dto.Fid, dto.Edit, dto.Required, dto.InputAttr) +
                $" value='{dto.Value}' rows='{dto.RowsCount}' style='width:{dto.Width}'" +
                _Helper.GetPlaceHolder(dto.InputTip) +
                _Helper.GetRequired(dto.Required) +
                _Helper.GetMaxLength(dto.MaxLen);

            //html
            var html = $"<textarea{attr} data-type='textarea' class='form-control xi-box {dto.BoxClass}'></textarea>";
            if (!_Str.IsEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, dto.Required, dto.LabelTip, dto.InRow, dto.Cols);
            return new HtmlString(html);
        }

    } //class
}
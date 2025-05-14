using Base.Services;
using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;

namespace BaseWeb.ViewComponents
{
    //readonly field
    public class XiReadViewComponent
    {
        public HtmlString Invoke(XiReadDto dto)
        {
            var attr = _Helper.GetInputAttr(dto.Fid, "", false, dto.InputAttr);
            if (_Str.NotEmpty(dto.Format))
                attr += $" data-format='{dto.Format}'";

            //xiRead 無條件加上 xg-inline
            //xi-read2 表示 edit style
            var css = "form-control xg-inline" + (dto.EditStyle ? " xi-read2" : " xi-read");
            if (dto.BoxClass != "")
                css += " " + dto.BoxClass;
            //add class xi-unsave for not save DB, _form.js toJson() will filter out it !!
            if (!dto.SaveDb)
                css += " xi-unsave";
            var html = $"<label{attr} data-type='read' class='form-control {css}'>{dto.Value}</label>";

            if (_Str.NotEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, false, dto.LabelTip, dto.InRow, dto.Cols);
            return new HtmlString(html);
        }

    }//class
}

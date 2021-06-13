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
            var attr = _Helper.GetInputAttr(dto.Fid, "", false, dto.ExtAttr);
            if (!string.IsNullOrEmpty(dto.Format))
                attr += $" data-format='{dto.Format}'";
            var html = $"<label{attr} data-type='read' class='form-control xi-read {dto.ExtClass}'>{dto.Value}</label>";

            if (!string.IsNullOrEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, false, dto.LabelTip, dto.InRow, dto.Cols);
            return new HtmlString(html);
        }

    }//class
}

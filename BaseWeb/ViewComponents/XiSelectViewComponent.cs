using Base.Services;
using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //select input
    public class XiSelectViewComponent : ViewComponent
    {
        public HtmlString Invoke(XiSelectDto dto)
        {
            /*
             string title, string fid, string value, List<IdStrDto> rows, 
            bool required = false, bool inRow = false, string edit = "", bool addEmptyRow = true,
            string labelTip = "", string inputTip = "", string extAttr = "", string extClass = "",
            string fnOnChange = "", string cols = "" 
             */
            var html = _Helper.GetSelectHtml(dto.Fid, dto.Value, "select", dto.Rows,
                dto.Required, dto.Edit, dto.AddEmptyRow,
                dto.InputTip, dto.InputAttr, dto.BoxClass, dto.FnOnChange);

            if (!_Str.IsEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, dto.Required, dto.LabelTip, dto.InRow, dto.Cols);
            return new HtmlString(html);
        } 

    }//class
}

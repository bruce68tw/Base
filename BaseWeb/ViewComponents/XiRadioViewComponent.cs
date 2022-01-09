using Base.Services;
using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

//TODO: modifing
namespace BaseWeb.ViewComponents
{
    //Radio button group, consider horizontal or vertical
    public class XiRadioViewComponent : ViewComponent
    {
        public HtmlString Invoke(XiRadioDto dto)
        {
            /*
             string title, string fid, string value, 
            List<IdStrDto> rows, bool inRow = false, string edit = "",
            string labelTip = "", string extClass = "", string extAttr = "",
            bool isHori = true, string cols = "",
            string fnOnChange = ""
             */
            //box & ext class
            //var boxClass = "xi-box"; 
            if (dto.IsHori)
                dto.BoxClass += " xg-inline";
            //if (extClass != "")
            //    extClass = " " + extClass;

            //default input this
            //prop.FnOnChange = _Helper.GetFnOnChange("onclick", prop, "this");

            //one radio (span for radio sign)
            var tplItem = @"
<label class='xi-check'>
	<input type='radio'{0}>{1}
	<span class='xi-rspan'></span>
</label>
";
            var list = "";
            for (var i=0; i< dto.Rows.Count; i++)
            {
                //change empty to nbsp, or radio will be wrong !!
                var row = dto.Rows[i];
                if (row.Str == "")
                    row.Str = "&nbsp;";

                //get attr, value attr will disappear, use data-value instead !!
                var attr = _Helper.GetInputAttr(dto.Fid, dto.Edit, false, dto.InputAttr) +
                    $" name='{dto.Fid}' data-value='{row.Id}' data-type='radio'" +
                    (_Str.IsEmpty(dto.FnOnChange) ? "" : $" onclick='{dto.FnOnChange}'") +
                    (row.Id == dto.Value ? " checked" : "");
                list += string.Format(tplItem, attr, row.Str);
            }

            //get html
            var html = $"<div class='xi-box {dto.BoxClass}'>{list}</div>";

            //add title outside
            //consider this field could in datatable(no title) !!
            if (!_Str.IsEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, false, dto.LabelTip, dto.InRow, dto.Cols, true);
            return new HtmlString(html);
        }

    }//class
}

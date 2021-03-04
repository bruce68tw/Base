using Base.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

//TODO: modifing
namespace BaseWeb.ViewComponents
{
    public class XiRadiosViewComponent : ViewComponent
    {
        /// <summary>
        /// Radio button group, consider horizontal or vertical
        /// </summary>
        /// <param name="title"></param>
        /// <param name="fid"></param>
        /// <param name="value"></param>
        /// <param name="rows"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public HtmlString Invoke(string title, string fid, string value, 
            List<IdStrDto> rows, bool inRow = false,  
            string labelTip = "", string extClass = "",
            bool isHori = true, string cols = "",
            string fnOnChange = "")
        {
            //prop ??= new PropRadioDto();

            //box class
            var boxClass = isHori ? "xg-inline" : "";
            //if (prop.BoxClass != "")
            //    boxClass += " " + prop.BoxClass;

            //ext class
            if (extClass != "")
                extClass = " " + extClass;

            //default input this
            //prop.FnOnChange = _Helper.GetFnOnChange("onclick", prop, "this");

            //one radio (span for radio sign)
            var htmlRow = @"
<label class='xg-radio{0}'>
	<input data-type='radio' type='radio'{1}>{2}
	<span></span>
</label>
";
            var list = "";
            for (var i=0; i<rows.Count; i++)
            {
                //change empty to nbsp, or radio will be wrong !!
                var row = rows[i];
                if (row.Str == "")
                    row.Str = "&nbsp;";

                //get attr, no consider readonly
                var attr = (i == 0 ? $" data-fid='{fid}'" : "") +
                    $" name='{fid}' value='{row.Id}'" +
                    (fnOnChange == "" ? "" : $" onclick='{fnOnChange}'") +
                    (row.Id == value ? " checked" : "");

                list += string.Format(htmlRow, extClass, attr, row.Str);
            }

            //get html
            var html = $@"
<div class='{boxClass}'>
    {list}
</div>";

            //add title outside
            //consider this field could in datatable(no title) !!
            if (!string.IsNullOrEmpty(title))
                html = _Helper.InputAddLayout(html, title, false, labelTip, inRow, cols);

            return new HtmlString(html);
        }

    }//class
}

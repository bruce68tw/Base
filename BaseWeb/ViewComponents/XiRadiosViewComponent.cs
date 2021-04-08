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
        /// <returns></returns>
        public HtmlString Invoke(string title, string fid, string value, 
            List<IdStrDto> rows, bool inRow = false,  
            string labelTip = "", string extClass = "",
            bool isHori = true, string cols = "",
            string fnOnChange = "")
        {
            //box & ext class
            //var boxClass = "xi-box"; 
            if (isHori)
                extClass += " xg-inline";
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
            for (var i=0; i<rows.Count; i++)
            {
                //change empty to nbsp, or radio will be wrong !!
                var row = rows[i];
                if (row.Str == "")
                    row.Str = "&nbsp;";

                //get attr, no consider readonly
                //value attr will disappear, use data-value instead !!
                var attr = (i == 0 ? $" data-fid='{fid}' data-type='radio'" : "") +
                    $" name='{fid}' data-value='{row.Id}'" +
                    (fnOnChange == "" ? "" : $" onclick='{fnOnChange}'") +
                    (row.Id == value ? " checked" : "");
                list += string.Format(tplItem, attr, row.Str);
            }

            //get html
            var html = $@"
<div class='xi-box {extClass}'>
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

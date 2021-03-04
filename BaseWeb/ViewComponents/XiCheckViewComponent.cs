using Base.Models;
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
        /// <param name="title"></param>
        /// <param name="fid"></param>
        /// <param name="isCheck"></param>
        /// <param name="value"></param>
        /// <param name="label">0表示在dt內</param>
        /// <param name="inRow"></param>
        /// <param name="cols"></param>
        /// <param name="labelTip"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public HtmlString Invoke(string title, string fid, string value = "1",
            bool isCheck = false, string label = "", bool inRow = false,
            bool editable = true, 
            string extAttr = "", string extClass = "",
            string cols = null, string labelTip = "", string fnOnChange = "")
        {
            //prop ??= new PropCheckDto();
            //if (label == "")
            //    label = "&nbsp;";   //add space, or position will wrong

            //get attr
            var attr = _Helper.GetInputAttr(fid, editable);
            if (isCheck)
                attr += " checked";
            if (fnOnChange != "")
                attr += $" onclick='{fnOnChange}'";

            //ext class
            //var extClass = (prop.ExtClass == "") ? "" : " " + prop.ExtClass;
            if (label == "")
                extClass += " xg-no-label";

            //get html (span for checkbox checked sign)
            //value attr will disappear, use data-value instead !!
            var html = $@"
<label class='xi-check {extClass}' {extAttr}>
    <input{attr} data-type='check' type='checkbox' data-value='{value}'>{label}
    <span></span>
</label>";

            //add title
            if (!string.IsNullOrEmpty(title))
                html = _Helper.InputAddLayout(html, title, false, labelTip, inRow, cols);

            return new HtmlString(html);
        }

    }//class
}
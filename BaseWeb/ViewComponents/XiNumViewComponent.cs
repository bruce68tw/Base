using Base.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XiNumViewComponent : ViewComponent
    {
        /// <summary>
        /// numeric input
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="fid"></param>
        /// <param name="value"></param>
        /// <param name="title">0表示在Datatable內</param>
        /// <param name="required"></param>
        /// <param name="inputCols"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public HtmlString Invoke(string title, string fid, string value = "", 
            bool required = false, bool inRow = false, 
            bool isDigit = true,
            string labelTip = "", string inputTip = "",
            bool editable = true, string width = "100%",
            string extAttr = "", string extClass = "",
            decimal? min = null, decimal? max = null,
            string cols = "")
        {
            //prop = prop ?? new PropNumDto();

            //attr
            var type = isDigit ? "digits" : "number";
            var attr = _Helper.GetInputAttr(fid, editable, required) +
                $" type='{type}' value='{value}' style='text-align:right; width:{width}'" +
                _Helper.GetRequired(required) +
                _Helper.GetPlaceHolder(inputTip);

            if (min != null)
                attr += " min='" + min + "'";
            if (max != null)
                attr += " max='" + max + "'";

            //html
            var html = $"<input{attr} data-type='num' class='form-control {extClass}' {extAttr}>";

            //add title
            if (!string.IsNullOrEmpty(title))
                html = _Helper.InputAddLayout(html, title, required, labelTip, inRow, cols);

            return new HtmlString(html);
        } 

    }//class
}

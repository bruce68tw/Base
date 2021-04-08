using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>
    /// text area
    /// </summary>
    public class XiHtmlViewComponent : ViewComponent
    {
        /// <summary>
        /// html input
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HtmlString Invoke(string title, string fid,
            string value = "",
            int maxLen = 0, int rowsCount = 10,
            bool required = false, bool editable = true, bool inRow = false,
            string labelTip = "", string inputTip = "",
            string extAttr = "", string extClass = "", string cols = "")
        {
            /*
            var html = _Helper.GetTextareaHtml(title, fid, "html", value,
                maxLen, rowsCount,
                required, editable, inRow,
                labelTip, inputTip,
                extAttr, extClass, cols);
            return new HtmlString(html);
            */
            var attr = _Helper.GetInputAttr(fid, editable, required) +
                $" value='{value}' rows='{rowsCount}' style='width:100%'" +
                _Helper.GetPlaceHolder(inputTip) +
                _Helper.GetRequired(required) +
                _Helper.GetMaxLength(maxLen);
            if (!string.IsNullOrEmpty(extAttr))
                attr += " " + extAttr;

            //html
            //summernote will add div below textarea, so add div outside for validate msg
            var html = string.Format($@"
<div class='xi-box {0}' {1}>
    <textarea{attr} data-type='html' class='form-control'></textarea>
</div>", extClass, extAttr);

            if (!string.IsNullOrEmpty(title))
                html = _Helper.InputAddLayout(html, title, required, labelTip, inRow, cols);
            return new HtmlString(html);
        }

    } //class
}
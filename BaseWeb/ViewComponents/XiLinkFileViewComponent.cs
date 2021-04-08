using Base.Services;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>
    /// file input
    /// </summary>
    public class XiLinkFileViewComponent : ViewComponent
    {
        /// <summary>
        /// file upload
        /// </summary>
        /// <param name="title"></param>
        /// <param name="fid"></param>
        /// <param name="cols"></param>
        /// <param name="fileType">I(image),E(excel),W(word)</param>
        /// <returns></returns>
        public HtmlString Invoke(string title, string fid, string value = "",
            bool inRow = false, string cols = "",
            string labelTip = "", string extAttr = "", string extClass = "",
            string fnOnClick = "_me.onViewFile(this)")
        {

            var html = $@"
<a href='#' data-fid='{fid}' data-type='linkFile' class='{extClass}' style='height:32px; display:table-cell; vertical-align:middle;' onclick='event.preventDefault(); {fnOnClick}' {extAttr}>{value}</a>
";
            //add label if need
            if (!string.IsNullOrEmpty(title))
                html = _Helper.InputAddLayout(html, title, false, labelTip, inRow, cols);

            return new HtmlString(html);
        }
        
    } //class
}

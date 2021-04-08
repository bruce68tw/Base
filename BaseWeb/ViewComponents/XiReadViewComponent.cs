using BaseWeb.Services;
using Microsoft.AspNetCore.Html;

namespace BaseWeb.ViewComponents
{
    public class XiReadViewComponent
    {
        /// <summary>
        /// display field
        /// </summary>
        /// <param name="title"></param>
        /// <param name="fid"></param>
        /// <param name="label"></param>
        /// <param name="inRow"></param>
        /// <param name="format">BrFormatEstr type for datetime/date</param>
        /// <param name="cols"></param>
        /// <returns></returns>
        public HtmlString Invoke(string title, string fid, 
            string label = "", bool inRow = false, string format = "", string cols = "")
        {
            var attr = _Helper.GetInputAttr(fid);
            if (format != "")
                attr += $" data-format='{format}'";
            var html = $"<label{attr} data-type='read' class='form-control xi-read'>{label}</label>";

            if (!string.IsNullOrEmpty(title))
                html = _Helper.InputAddLayout(html, title, false, "", inRow, cols);

            return new HtmlString(html);
        }

    }//class
}

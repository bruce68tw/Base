using Base.Services;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //配合 _xp.js
    public class XgThViewComponent : ViewComponent
    {
        /// <summary>
        /// table th
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="title">prog path list</param>
        /// <param name="tip"></param>
        /// <param name="required"></param>
        /// <returns></returns>
        public HtmlString Invoke(string title, string tip = "", bool required = false, string extClass = "")
        {
            title = _Helper.GetRequiredSpan(required) + title;
            var html = (tip == "")
                ? "<th{0}>" + title + "</th>"
                : "<th{0} title='" + tip + "'>" + title + "<i class='ico-info'></i></th>";
            if (!string.IsNullOrEmpty(extClass))
                extClass = " class='" + extClass + "'";
            return new HtmlString(string.Format(html, extClass));
        }

    } //class
}
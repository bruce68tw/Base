using Base.Services;
using BaseWeb.Models;
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
        public HtmlString Invoke(XgThDto dto)
        {
            var title = _Helper.GetRequiredSpan(dto.Required) + dto.Title;
            var html = (dto.Tip == "")
                ? "<th{0}>" + title + "</th>"
                : "<th{0} title='" + dto.Tip + "'>" + title + "<i class='ico-info'></i></th>";
            var attr = dto.ExtClass;
            if (dto.HideRwd)
                attr += " " + _Fun.HideRwd;
            if (!string.IsNullOrEmpty(attr))
                attr = " class='" + attr + "'";
            if (dto.MinWidth > 0)
                attr += $" style='min-width:{dto.MinWidth}px'";
            return new HtmlString(string.Format(html, attr));
        }

    } //class
}
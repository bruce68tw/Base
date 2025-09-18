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
            var title = _Input.GetRequiredSpan(dto.Required) + dto.Title;
            var html = (dto.Tip == "")
                ? "<th{0}>" + title + "</th>"
                : "<th{0} title='" + dto.Tip + "'>" + title + "<i class='ico-info'></i></th>";

            //class
            var cls = dto.ClsExt;
            if (dto.MinWidth > 0)
                cls += $" g-iw{dto.MinWidth}";
            if (dto.HideRwd)
                cls += " " + _Fun.ClsHideRwd;
            if (cls != "")
                cls = $" class='{cls}'";
            return new HtmlString(string.Format(html, cls));
        }

    } //class
}
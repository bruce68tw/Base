using Base.Services;
using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //配合 _xp.js
    /// <summary>呈現可附提示及必填標記的表頭的 MVC ViewComponent。</summary>
    public class XgThViewComponent : ViewComponent
    {
        /// <summary>產生可附提示及必填標記的表頭的 HTML。</summary>
        /// <param name="dto">表頭的標題、提示與顯示設定。</param>
        /// <returns></returns>
        public HtmlString Invoke(XgThDto dto)
        {
            var title = _Input.GetReqSpan(dto.Required) + dto.Title;
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
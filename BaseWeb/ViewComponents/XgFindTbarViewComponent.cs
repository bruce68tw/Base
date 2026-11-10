using AngleSharp.Dom;
using Base.Services;
using BaseApi.Services;
using BaseWeb.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

//查詢畫面右上方工具列
namespace BaseWeb.ViewComponents
{
    //配合 _xp.js
    /// <summary>呈現包含查詢及選用操作的查詢工具列的 MVC ViewComponent。</summary>
    public class XgFindTbarViewComponent : ViewComponent
    {
        /// <summary>產生包含查詢及選用操作的查詢工具列的 HTML。 (path) (tool)</summary>
        /// <param name="dto">查詢工具列的版面和按鈕設定，可省略。</param>
        /// <returns></returns>
        public HtmlString Invoke(XgFindTbarDto? dto = null)
        {
            dto ??= new XgFindTbarDto();

            /*
            var fnOnFind = _Str.IsEmpty(dto.FnWhenFind)
                ? "_me.crudR.onFind()"
                : dto.FnWhenFind;
            */

            //set toolbar buttons, 使用string.format, 無法使用 $"" !!
            var baseR = _Locale.GetBaseRes();
            var html = $"<button id='btnFind' type='button' class='btn x-btn1 xd-read' data-onclick='_me.crudR.onFind'>{baseR.BtnFind}<i class='ico-find'></i></button>";
            if (dto.HasReset)
                html += $"<button type='button' class='btn x-btn-cancel' data-onclick='_me.crudR.onResetFind'>{baseR.BtnReset}<i class='ico-delete'></i></button>";
            if (dto.HasFind2)
                html += $"<button type='button' class='btn btn-other' data-onclick='_me.crudR.onFind2'>{baseR.BtnFind2}<i class='ico-find2'></i></button>";

            if (dto.IsHori)
                //html = "<span class='col-md-3 x-find-tbar'>" + html + "</span>";
                html = dto.SpanMode
                    ? $@"
<div class='x-find-tbar2'>
    {html}
</div>"
                    : $@"
<div class='col-md-3 d-flex ps-0 pb-1'>
    <div class='x-find-tbar'>
        {html}
    </div>
</div>";
            else
            {
                //vertical
                html = $@"
<div class='col-md-3'>
    <div></div>
    <div>
        {html}
    </div>
</div>";
            }            
            return new HtmlString(html);
        }

    } //class
}
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
    public class XgFindTbarViewComponent : ViewComponent
    {
        /// <summary>
        /// program title: left(path), right(tool)
        /// </summary>
        /// <param name="dto"></param>
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
            var html = string.Format("<button type='button' class='btn btn-primary xd-read' onclick='_me.crudR.onFind()'>{0}<i class='ico-find'></i></button>", baseR.BtnFind);
            if (dto.HasReset)
                html += string.Format("<button type='button' class='btn btn-secondary' onclick='_me.crudR.onResetFind()'>{0}<i class='ico-delete'></i></button>", baseR.BtnReset);
            if (dto.HasFind2)
                html += string.Format("<button type='button' class='btn btn-success' onclick='_me.crudR.onFind2()'>{0}<i class='ico-find2'></i></button>", baseR.BtnFind2);

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
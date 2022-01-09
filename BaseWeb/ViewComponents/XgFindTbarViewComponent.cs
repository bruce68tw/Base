using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

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
        public HtmlString Invoke(XgFindTbarDto dto = null)
        {
            if (dto == null)
                dto = new XgFindTbarDto();

            //set toolbar buttons
            var baseR = _Locale.GetBaseRes();
            var html = string.Format("<button type='button' class='btn xg-btn-size btn-primary' onclick='_crud.onFind()'>{0}<i class='ico-find'></i></button>", baseR.BtnFind);
            if (dto.HasReset)
                html += string.Format("<button type='button' class='btn xg-btn' onclick='_crud.onResetFind()'>{0}<i class='ico-delete'></i></button>", baseR.BtnReset);
            if (dto.HasFind2)
                html += string.Format("<button type='button' class='btn xg-btn-size btn-success' onclick='_crud.onFind2()'>{0}<i class='ico-find2'></i></button>", baseR.BtnFind2);

            if (dto.IsHori)
            {
                //horizontal
                html = "<span class='col-md-3 xg-find-tbar'>" + html + "</span>";
            }
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
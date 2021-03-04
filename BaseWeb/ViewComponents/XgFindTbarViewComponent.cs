using Base.Services;
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
        /// <param name="helper"></param>
        /// <param name="names">prog path list</param>
        /// <param name="status">按鈕狀態(0/1), 依次為: 查詢(不必設定),重設(傳入),展開(傳入)</param>
        /// <returns></returns>
        public HtmlString Invoke(bool isHorizontal = true, bool hasReset = false, bool hasFind2 = false)
        {
            //set toolbar buttons
            //var rb = _Locale.RB;
            var baseR = _Fun.GetBaseRes();
            var html = string.Format("<button type='button' class='btn xg-btn-size btn-primary' onclick='_crud.onFind()'>{0}<i class='ico-find'></i></button>", baseR.BtnFind);
            if (hasReset)
                html += string.Format("<button type='button' class='btn xg-btn' onclick='_crud.onReset()'>{0}<i class='ico-delete'></i></button>", baseR.BtnReset);
            if (hasFind2)
                html += string.Format("<button type='button' class='btn xg-btn-size btn-success' onclick='_crud.onFind2()'>{0}<i class='ico-find2'></i></button>", baseR.BtnFind2);

            if (isHorizontal)
            {
                //horizontal
                html = "<span class='xg-find-tbar'>" + html + "</span>";
            }
            else
            {
                //vertical
                html = string.Format(@"
<div class='col-md-3'>
    <div></div>
    <div>
        {0}
    </div>
</div>
", html);
            }
            
            return new HtmlString(html);
        }

    } //class
}
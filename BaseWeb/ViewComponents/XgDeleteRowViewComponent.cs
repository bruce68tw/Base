using Base.Services;
using BaseApi.Services;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //add row button
    /// <summary>呈現刪除編輯列的按鈕的 MVC ViewComponent。</summary>
    public class XgDeleteRowViewComponent : ViewComponent
    {
        /// <summary>產生刪除編輯列的按鈕的 HTML。</summary>
        /// <param name="fnOnClick">按鈕點擊時要呼叫的前端函式</param>
        public HtmlString Invoke(string fnOnClick)
        {
            //var rb = _Locale.RB;
            var html = string.Format(@"
<button type='button' data-onclick='{0}' class='btn' data-edit>
    <i class='ico-delete x-delete' title='{1}'></i>
</button>", fnOnClick, _Locale.GetBaseRes().TipDeleteRow);

            return new HtmlString(html);
        }        

    }//class
}

using BaseApi.Services;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //add row button
    /// <summary>呈現新增資料列按鈕的 MVC ViewComponent。</summary>
    public class XgAddRowViewComponent : ViewComponent
    {
        /// <summary>產生新增資料列按鈕的 HTML。</summary>
        /// <param name="fnOnClick">按鈕點擊時要呼叫的前端函式</param>
        public HtmlString Invoke(string fnOnClick)
        {
            //todo: ??
            var attr = _Input.GetEventAttr("onclick", fnOnClick);
            var html = $@"
<button type='button' {attr} class='btn x-btn2' data-edit>{_Locale.GetBaseRes().BtnAddRow}
    <i class='ico-plus'></i>
</button>
";
            return new HtmlString(html);
        }
    }//class
}

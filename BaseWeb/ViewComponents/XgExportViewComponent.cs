using BaseApi.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>呈現資料匯出按鈕的 MVC ViewComponent。</summary>
    public class XgExportViewComponent : ViewComponent
    {
        /// <summary>產生資料匯出按鈕的 HTML。</summary>
        /// <param name="fnOnClick">按鈕點擊時要呼叫的前端函式</param>
        public HtmlString Invoke(string fnOnClick = "_me.crudR.onExport")
        {
            //var rb = _Locale.RB;
            var html = $"<button type='button' class='btn x-btn-other' data-onclick='{fnOnClick}'>{_Locale.GetBaseRes().BtnExport}<i class='ico-excel'></i></button>";
            return new HtmlString(html);
        }

    } //class
}
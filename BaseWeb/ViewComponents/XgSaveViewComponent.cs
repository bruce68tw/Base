using BaseApi.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>呈現單一儲存按鈕的 MVC ViewComponent。</summary>
    public class XgSaveViewComponent : ViewComponent
    {
        /// <summary>產生單一儲存按鈕的 HTML。</summary>
        /// <param name="align">按鈕區塊的水平對齊設定</param>
        /// <param name="fnOnSave">按下儲存按鈕時呼叫的前端函式</param>
        public HtmlString Invoke(string align = "center", string fnOnSave = "_me.crudE.onSave()")
        {
            var baseR = _Locale.GetBaseRes();
            var html = $@"
<div class='x-{align}'>
    <button id='btnSave' type='button' class='btn x-btn1' data-edit data-onclick='{fnOnSave}'>{baseR.BtnSave}<i class='ico-save'></i></button>
</div>
";
            return new HtmlString(html);
        }

    } //class
}
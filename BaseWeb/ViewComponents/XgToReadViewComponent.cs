using BaseApi.Services;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //add row button
    /// <summary>呈現返回列表的按鈕的 MVC ViewComponent。</summary>
    public class XgToReadViewComponent : ViewComponent
    {
        /// <summary>產生返回列表的按鈕的 HTML。</summary>
        /// <param name="label">畫面顯示的標籤文字</param>
        public HtmlString Invoke(string label = "")
        {
            if (string.IsNullOrEmpty(label))
                label = _Locale.GetBaseRes().BtnToRead;

            var html = $@"
<button id='btnToRead' type='button' class='btn x-btn-cancel' data-onclick='_me.crudR.onToRead'>{label}
    <i class='ico-back'></i>
</button>
";
            return new HtmlString(html);
        }
    }//class
}

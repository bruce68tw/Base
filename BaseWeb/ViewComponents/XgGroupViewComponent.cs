using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>呈現表單群組標題的 MVC ViewComponent。</summary>
    public class XgGroupViewComponent : ViewComponent
    {
        /// <summary>產生表單群組標題的 HTML。</summary>
        /// <param name="label">畫面顯示的標籤文字</param>
        /// <param name="icon">是否附加群組圖示</param>
        public HtmlString Invoke(string label, bool icon = false)
        {
            return new HtmlString(_Input.XgGroup(label, icon));
        }

    } //class
}
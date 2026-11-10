using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BaseWeb.Helpers
{
    //field title, no row container
    //title is before field, label is after field
    /// <summary>提供帶有選用必填標記的舊版標題輔助方法。</summary>
    public static class XgTitleHelper
    {
        /// <summary>依標籤、必填設定與版面欄寬產生標題 HTML。</summary>
        /// <param name="htmlHelper">目前 Razor 檢視使用的 HTML 輔助物件</param>
        /// <param name="label">畫面顯示的標籤文字</param>
        /// <param name="required">是否標為必填</param>
        /// <param name="cols">傳給元件的版面欄寬設定</param>
        public static IHtmlContent XgTitle(this IHtmlHelper htmlHelper, string label, bool required = false, int cols = 2)
        {
            var reqStr = required ? "<span class='xg-required'>*</span>" : "";
            var html = string.Format(@"
<div class='col-md-{1} xg-label'>{0}
    {2}
</div>
", label, cols, reqStr);

            return new HtmlString(html);
        }

    }//class
}

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BaseWeb.Helpers
{
    /// <summary>提供表格表頭的全選核取方塊。</summary>
    public static class XiCheckAllHelper
    {
        /// <summary>產生全選核取方塊，點擊時呼叫 _crud.onCheckAll。 (edit form) ()</summary>
        /// <param name="htmlHelper">目前 Razor 檢視使用的 HTML 輔助物件</param>
        /// <param name="boxStr">要放在全選核取方塊旁的文字</param>
        /// <param name="dataFid">全選核取方塊用來辨識對應欄位的值 (data-id value)</param>
        /// <returns></returns>
        public static IHtmlContent XiCheckAll(this IHtmlHelper htmlHelper, string boxStr, string dataFid)
        {
            //tail span is need for checkbox
            var html = string.Format(@"
<th width='60' class='text-center'>
    <label class='xg-check'>&nbsp
        <input type='checkbox' class='checkboxes' onclick='_crud.onCheckAll(this, {0}, &quot;{1}&quot;)'>
        <span></span>
    </label>
</th>
", boxStr, dataFid);

            return new HtmlString(html);
        }

    } //class
}

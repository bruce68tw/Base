using BaseWeb.Services;
using BaseWeb.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BaseWeb.Helpers
{
    /// <summary>提供編輯表單使用的單一單選項目輔助方法。</summary>
    public static class XiRadioHelper
    {

        /// <summary>依欄位、選取狀態和值產生單一單選項目。</summary>
        /// <param name="htmlHelper">目前 Razor 檢視使用的 HTML 輔助物件</param>
        /// <param name="fid">要處理的欄位識別值</param>
        /// <param name="isCheck">是否預先選取此項目</param>
        /// <param name="value">用於初始化欄位或選項的值</param>
        /// <param name="label">畫面顯示的標籤文字</param>
        /// <param name="prop">單選項目的額外欄位設定</param>
        /// <returns></returns>
        public static IHtmlContent XiRadio(this IHtmlHelper htmlHelper, string fid = "", bool isCheck = false, string value = "1", string label = "", PropInputDto prop = null)
        {
            if (prop == null)
                prop = new PropInputDto();

            //adjust
            if (label == "")
                label = "&nbsp;";   //add space, or position will wrong !!

            //attr
            //第3個參數 isInDt: false
            var attr = _Helper.GetBaseAttr(fid, prop) +
                " value='" + value + "'" +
                _Helper.GetFnOnChange("onclick", prop, "this.checked");
            if (isCheck)
                attr += " checked";

            /*
            //box class
            var boxClass = prop.Inline ? "xg-inline" : "";
            if (prop.BoxClass != "")
                boxClass += " " + prop.BoxClass;
            */

            //ext class
            //var extClass = (prop.ExtClass == "") ? "" : " " + prop.ExtClass;
            //if (prop.InDt)
            //    extClass += " xg-in-dt";

            //沒有 box container, 只有radio !!
            var html = string.Format(@"
<label class='xg-radio {0}'>
    <input type='radio'{1}>{2}
    <span></span>
</label>
", prop.ExtClass, attr, label);

            return new HtmlString(html);
        }

    }//class
}
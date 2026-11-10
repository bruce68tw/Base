using BaseApi.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //open modal for table
    //link with button style(always enabled)
    /// <summary>呈現開啟欄位編輯對話框的連結的 MVC ViewComponent。</summary>
    public class XgOpenModalViewComponent : ViewComponent
    {
        /// <summary>產生開啟欄位編輯對話框的連結的 HTML。</summary>
        /// <param name="title">圖表資料集的顯示名稱</param>
        /// <param name="fid">要處理的欄位識別值</param>
        /// <param name="required">是否標為必填</param>
        /// <param name="maxLen">文字長度上限</param>
        public HtmlString Invoke(string title, string fid, bool required, int maxLen)
        {
            var btnOpen = _Locale.GetBaseRes().BtnOpen;
            var req = required ? "true" : "false";
            var html = $"<a data-onclick='_me.crudE.onOpenModal' data-args='{title},{fid},{req},{maxLen}' class='btn btn-outline-secondary btn-sm'>{btnOpen}</a>";
            return new HtmlString(html);
        }        

    }//class
}

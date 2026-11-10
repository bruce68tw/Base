using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

//只有一個 radio 按鈕, 自行組合
namespace BaseWeb.ViewComponents
{
    /// <summary>呈現單一單選項目的 MVC ViewComponent。</summary>
    public class XiRadio0ViewComponent : ViewComponent
    {
        /// <summary>產生單一單選項目的 HTML。</summary>
        /// <param name="fid">要處理的欄位識別值</param>
        /// <param name="value">用於初始化欄位或選項的值</param>
        /// <param name="label">畫面顯示的標籤文字</param>
        public HtmlString Invoke(string fid, string value, string label)
        {
            return new HtmlString(_Input.XiRadio0(fid, value, label));
        }

    }//class
}

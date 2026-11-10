using Microsoft.AspNetCore.Html;
using System;

namespace BaseWeb.Services
{
    /// <summary>產生 Razor 頁面載入模組腳本使用的屬性。</summary>
    public static class _Fun3
    {
        //for 資安 script, style 使用 inline
        public static string Nonce = "";

        //傳回 script src 文字, 加上時間序號做為url後面亂數
        /// <summary>組合模組腳本路徑、nonce 與時間戳記查詢參數。</summary>
        /// <param name="prog">對應 jsView 目錄下的腳本名稱</param>
        public static IHtmlContent Script(string prog)
        {
            var time = DateTime.Now.ToString("HHmmssfff");
            return new HtmlString($"type=\"module\" nonce=\"{Nonce}\" src=\"/jsView/{prog}.js?v={time}\"");
        }

    }//class
}

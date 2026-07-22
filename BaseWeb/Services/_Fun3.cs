using Microsoft.AspNetCore.Html;
using System;

namespace BaseWeb.Services
{
    public static class _Fun3
    {
        //for 資安 script, style 使用 inline
        public static string Nonce = "";

        //傳回 script src 文字, 加上時間序號做為url後面亂數
        public static IHtmlContent Script(string prog)
        {
            var time = DateTime.Now.ToString("HHmmssfff");
            return new HtmlString($"type=\"module\" nonce=\"{Nonce}\" src=\"/jsView/{prog}.js?v={time}\"");
        }

    }//class
}

using Base.Services;
using Microsoft.AspNetCore.Html;
using System;
using System.IO;

namespace BaseApi.Services
{
    /// <summary>
    /// 提供網站目錄、CRUD 上傳欄位名稱及將串流直接寫入目前 HTTP 回應的下載工具。
    /// </summary>
    public static class _Fun2
    {
        //伺服器端附件對應欄位，名稱以底線開頭。
        //記錄上傳欄位與資料列鍵值的對應。
        /// <summary>
        /// CRUD 上傳資料中的附件對應欄位，記錄上傳欄位與資料列鍵值的對應。
        /// </summary>
        public const string FileJson = "_fileJson";

        /// <summary>
        /// 網站目錄路徑，尾端含目錄分隔符號。
        /// 初始化時優先使用存在的 wwwroot 目錄，否則沿用應用根目錄。
        /// </summary>        
        public static string DirWeb = "";

        /// <summary>
        /// 由 DirRoot 初始組成的範本目錄路徑。
        /// </summary>
        public static string DirTpl = "";

        //public const string SessionFid = "ASP.NET_SessionId";
        //public const string SessionFid = "_SessionId_";
        //public const string LocaleFid = "_Locale_";     //cookie field id for locale
        //public static string LocaleFid = CookieRequestCultureProvider.DefaultCookieName;     //cookie field id for locale

        //public static string NoImagePath = "";

        //for 資安 script, style 使用 inline
        public static string Nonce = "";


        //constructor
        static _Fun2()
        {
            var dir = _Fun.DirRoot + "wwwroot/";
            DirWeb = Directory.Exists(dir) ? dir : _Fun.DirRoot;
            DirTpl = _Fun.DirRoot + "_template" + _Fun.DirSep;

            //NoImagePath = DirWeb + "image/noImage.jpg";
        }

        /// <summary>
        /// get template file path
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="hasLocale"></param>
        /// <returns></returns>
        public static string GetTplPath(string fileName, bool hasLocale)
        {
            return $"{DirTpl}{(hasLocale ? _Locale.GetLocale() : "")}/{fileName}";
        }

        //傳回 script src 文字, 加上時間序號做為url後面亂數
        /// <summary>組合模組腳本路徑、nonce 與時間戳記查詢參數。</summary>
        /// <param name="prog">對應 jsView 目錄下的腳本名稱</param>
        public static IHtmlContent Script(string prog)
        {
            var time = DateTime.Now.ToString("HHmmssfff");
            return new HtmlString($"type=\"module\" nonce=\"{Nonce}\" src=\"/jsView/{prog}.js?v={time}\"");
        }

        /*
        //get cookies
        public static IRequestCookieCollection GetCookies()
        {
            return GetRequest().Cookies;
        }
        */

        /*
        /// <summary>
        /// move to _Http.RespStreamToFileA
        /// 需要可定位的串流及有效的 HTTP 請求。將串流位置重設為零，並以附件直接寫入目前回應。
        /// </summary>
        /// <param name="stream">要下載的可定位串流，複製前會移回開頭。</param>
        /// <param name="fileName">寫入 Content-Disposition 的下載名稱。</param>
        public static async Task ExportByStreamA(Stream stream, string fileName)
        {
            //response to client, must close docx first, 
            //so put code here, or docx file will get wrong !!
            var resp = _Http.GetResponse();

            //consider IE
            //resp.AppendHeader("Content-Disposition", "attachment;filename=" + fileName);
            var browser = _Http.GetRequest().Headers["User-Agent"].ToString();
            if (browser != null && browser.Equals("ie", StringComparison.OrdinalIgnoreCase))
                resp.Headers.Append("Content-Disposition", "attachment; filename*=UTF-8''" + HttpUtility.UrlPathEncode(fileName) + "\"");
            else
                resp.Headers.Append("Content-Disposition", "attachment; filename=\"" + HttpUtility.UrlPathEncode(fileName) + "\"");

            var ext = _File.GetFileExt(fileName);
            //resp.ContentType = _Http.ExtToContentType(ext, true); //會開啟檔案
            resp.ContentType = "application/octet-stream";  //強制下載, 但無作用, 還是會開啟!!

            //stream.Flush();
            stream.Position = 0;
            await stream.CopyToAsync(resp.Body);
            //resp.Flush();
            await resp.Body.FlushAsync();
            //resp.End();
            //resp.Body..EndWrite();
        }
        */

    }//class
}

using Base.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace BaseApi.Services
{
#pragma warning disable CA2211 // 非常數欄位不應可見
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

        //public const string SessionFid = "ASP.NET_SessionId";
        //public const string SessionFid = "_SessionId_";
        //public const string LocaleFid = "_Locale_";     //cookie field id for locale
        //public static string LocaleFid = CookieRequestCultureProvider.DefaultCookieName;     //cookie field id for locale

        //public static string NoImagePath = "";

        //constructor
        static _Fun2()
        {
            var dir = _Fun.DirRoot + "wwwroot/";
            DirWeb = Directory.Exists(dir)
                ? dir : _Fun.DirRoot;
            //NoImagePath = DirWeb + "image/noImage.jpg";
        }

        /*
        //get cookies
        public static IRequestCookieCollection GetCookies()
        {
            return GetRequest().Cookies;
        }
        */

        /// <summary>
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

    }//class
    #pragma warning restore CA2211 // 非常數欄位不應可見
}

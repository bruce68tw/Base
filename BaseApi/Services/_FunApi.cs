using Base.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace BaseApi.Services
{
#pragma warning disable CA2211 // 非常數欄位不應可見
    public static class _FunApi
    {
        //server side fid for file input collection, must pre '_'
        //key-value of file serverFid vs row key
        public const string FileJson = "_fileJson";

        /// <summary>
        /// dir web, has right slash
        /// </summary>        
        public static string DirWeb = "";

        //public const string SessionFid = "ASP.NET_SessionId";
        //public const string SessionFid = "_SessionId_";
        //public const string LocaleFid = "_Locale_";     //cookie field id for locale
        //public static string LocaleFid = CookieRequestCultureProvider.DefaultCookieName;     //cookie field id for locale

        //public static string NoImagePath = "";

        //constructor
        static _FunApi()
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
        /// export/response file by stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
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
            resp.ContentType = _Http.GetContentTypeByExt(ext);
            /*
            //resp.ContentType = "application/vnd.ms-word.document";
            if (ext == "doc" || ext == "docx")
                resp.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            else if(ext == "xls" || ext == "xlsx")
                resp.ContentType = "application/ms-excel";
            else
                resp.ContentType = "text/plain";
            */

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

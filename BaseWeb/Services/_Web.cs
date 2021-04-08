using Base.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Web;

namespace BaseWeb.Services
{
    public static class _Web
    {
        //server side fid for file input collection, must pre '_'
        //key-value of file serverFid vs row key
        public const string FileJson = "_fileJson";

        //input field error validation, need match client side _fun.js

        //public const string ErrTail = "_err";           //?? error label id
        //public const string ErrCls = "xg-error";            //error flag
        //public const string ErrLabCls = "xg-error-label";   //?? error label

        //public const string DtTitle = "_dt";   //htmlHelper 的title如果為此值, 表示在dt內

        //for controller return JsonResult
        //public const string AppJson = "application/json";

        //public const string SessionFid = "ASP.NET_SessionId";
        //public const string SessionFid = "_SessionId_";
        public const string LocaleFid = "_Locale_";     //cookie field id for locale

        public static HttpContext GetHttp()
        {
            var service = (IHttpContextAccessor)_Fun.GetDI().GetService(typeof(IHttpContextAccessor));
            return service.HttpContext;
        }

        //get http request
        public static HttpRequest GetRequest()
        {
            return GetHttp().Request;
        }

        //get http response
        public static HttpResponse GetResponse()
        {
            return GetHttp().Response;
        }

        //get cookies
        public static IRequestCookieCollection GetCookies()
        {
            return GetRequest().Cookies;
        }

        public static ISession GetSession()
        {
            return GetHttp().Session;
        }

        /// <summary>
        /// response stream to screen
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        public static void StreamToScreen(Stream stream, string fileName)
        {
            //response to client, must close docx first, 
            //so put code here, or docx file will get wrong !!
            var response = GetResponse();

            //consider IE
            //response.AppendHeader("Content-Disposition", "attachment;filename=" + fileName);
            var browser = GetRequest().Headers["User-Agent"].ToString();
            if (browser != null && browser.Equals("ie", StringComparison.OrdinalIgnoreCase))
                response.Headers.Append("Content-Disposition", "attachment; filename*=UTF-8''" + HttpUtility.UrlPathEncode(fileName) + "\"");
            else
                response.Headers.Append("Content-Disposition", "attachment; filename=\"" + HttpUtility.UrlPathEncode(fileName) + "\"");

            var ext = _File.GetFileExt(fileName);
            //response.ContentType = "application/vnd.ms-word.document";
            if (ext == ".doc" || ext == ".docx")
                response.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            else if(ext == ".xls" || ext == ".xlsx")
                response.ContentType = "application/ms-excel";

            //stream.Flush();
            stream.Position = 0;
            stream.CopyToAsync(response.Body);
            //response.Flush();
            response.Body.FlushAsync();
            //response.End();
            //response.Body..EndWrite();
        }

    }//class
}

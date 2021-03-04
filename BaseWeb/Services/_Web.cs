using Base.Services;
using Microsoft.AspNetCore.Http;

namespace BaseWeb.Services
{
    public static class _Web
    {
        //server side fid for file input collection, must pre '_'
        //key-value of file serverFid vs row key
        public const string FileJson = "_fileJson";

        //input field error validation, need match client side _fun.js

        public const string ErrTail = "_err";           //?? error label id
        public const string ErrCls = "xg-error";            //error flag
        public const string ErrLabCls = "xg-error-label";   //?? error label

        //public const string DtTitle = "_dt";   //htmlHelper 的title如果為此值, 表示在dt內

        //for controller return JsonResult
        public const string AppJson = "application/json";

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

    }//class
}

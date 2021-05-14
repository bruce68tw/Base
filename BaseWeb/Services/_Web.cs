using Base.Enums;
using Base.Models;
using Base.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace BaseWeb.Services
{
    public static class _Web
    {
        //server side fid for file input collection, must pre '_'
        //key-value of file serverFid vs row key
        public const string FileJson = "_fileJson";

        //public const string SessionFid = "ASP.NET_SessionId";
        //public const string SessionFid = "_SessionId_";
        //public const string LocaleFid = "_Locale_";     //cookie field id for locale
        public static string LocaleFid = CookieRequestCultureProvider.DefaultCookieName;     //cookie field id for locale

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

        /*
        //get cookies
        public static IRequestCookieCollection GetCookies()
        {
            return GetRequest().Cookies;
        }
        */

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
            var resp = GetResponse();

            //consider IE
            //resp.AppendHeader("Content-Disposition", "attachment;filename=" + fileName);
            var browser = GetRequest().Headers["User-Agent"].ToString();
            if (browser != null && browser.Equals("ie", StringComparison.OrdinalIgnoreCase))
                resp.Headers.Append("Content-Disposition", "attachment; filename*=UTF-8''" + HttpUtility.UrlPathEncode(fileName) + "\"");
            else
                resp.Headers.Append("Content-Disposition", "attachment; filename=\"" + HttpUtility.UrlPathEncode(fileName) + "\"");

            var ext = _File.GetFileExt(fileName);
            //resp.ContentType = "application/vnd.ms-word.document";
            if (ext == ".doc" || ext == ".docx")
                resp.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            else if(ext == ".xls" || ext == ".xlsx")
                resp.ContentType = "application/ms-excel";
            else
                resp.ContentType = "text/plain";

            //stream.Flush();
            stream.Position = 0;
            stream.CopyToAsync(resp.Body);
            //resp.Flush();
            resp.Body.FlushAsync();
            //resp.End();
            //resp.Body..EndWrite();
        }

        /// <summary>
        /// get prog menu by session, called by _Layout.cshtml for show menu
        /// </summary>
        /// <returns></returns>
        public static List<MenuDto> GetMenu()
        {
            //get authStrs
            var data = new List<MenuDto>();
            var authStrs = _Fun.GetBaseUser().ProgAuthStrs;
            if (string.IsNullOrEmpty(authStrs))
                return data;

            //get list
            var list = new List<string>();
            switch (_Fun.GetAuthType())
            {
                case AuthTypeEnum.Ctrl:
                    //do nothing
                    list = _Str.ToList(authStrs);
                    break;

                case AuthTypeEnum.Action:
                    //var list2 = new List<string>();
                    foreach (var item in list)
                        list.Add(_Str.GetLeft(item, ":"));
                    break;

                default:
                    return data;
            }

            //read Prog
            var sql = string.Format(@"
select Code, Name, Url, Sort
from dbo.XpProg
where Code in ({0})
order by Sort
", _List.ToStr(list, true));
            return _Db.GetModels<MenuDto>(sql);
        }

    }//class
}

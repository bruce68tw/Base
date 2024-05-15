using Base.Enums;
using Base.Models;
using Base.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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

        /// <summary>
        /// get prog menu from session, called by _Layout.cshtml for show menu
        /// </summary>
        /// <param name="locale">consider multiple language if not empty</param>
        /// <returns>return [] if null</returns>
        public static async Task<List<MenuDto>> GetMenuA()
        {
            //get authStrs
            var data = new List<MenuDto>();
            var baseUser = _Fun.GetBaseUser();
            var authStrs = baseUser.ProgAuthStrs;
            if (_Str.IsEmpty(authStrs))
                return data;

            //remove ',' at start/end
            authStrs = authStrs[1..^1];

            //get prog string list
            var progList = new List<string>();
            switch (_Fun.AuthType)
            {
                case AuthTypeEnum.Ctrl:
                    //do nothing
                    progList = _Str.ToList(authStrs);
                    break;

                case AuthTypeEnum.Action:
                case AuthTypeEnum.Row:
                    var list = authStrs.Split(',');
                    foreach (var item in list)
                        progList.Add(_Str.GetLeft(item, ":"));
                    break;

                default:
                    return data;
            }

            //get Program name from XpProg
            var sql = $@"
select Code, Name, Url, Sort
from dbo.XpProg
where Code in ({_List.ToStr(progList!, true)})
order by Sort
";
            return await _Db.GetModelsA<MenuDto>(sql) ?? new();
        }

    }//class
    #pragma warning restore CA2211 // 非常數欄位不應可見
}

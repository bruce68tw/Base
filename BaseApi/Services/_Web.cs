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
    public static class _Web
    {
        //server side fid for file input collection, must pre '_'
        //key-value of file serverFid vs row key
        public const string FileJson = "_fileJson";

        /// <summary>
        /// dir web, has right slash
        /// </summary>        
        public static string DirWeb = _Fun.DirRoot + "wwwroot/";

        //public const string SessionFid = "ASP.NET_SessionId";
        //public const string SessionFid = "_SessionId_";
        //public const string LocaleFid = "_Locale_";     //cookie field id for locale
        //public static string LocaleFid = CookieRequestCultureProvider.DefaultCookieName;     //cookie field id for locale

        public static string NoImagePath = DirWeb + "image/noImage.jpg";

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
        public static async Task ExportByStream(Stream stream, string fileName)
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
        /// get prog menu by session, called by _Layout.cshtml for show menu
        /// </summary>
        /// <param name="locale">consider multiple language if not empty</param>
        /// <returns></returns>
        public static async Task<List<MenuDto>> GetMenuAsync()
        {
            //get authStrs
            var data = new List<MenuDto>();
            var authStrs = _Fun.GetBaseUser().ProgAuthStrs;
            if (_Str.IsEmpty(authStrs))
                return data;

            //get prog string list
            var progList = new List<string>();
            switch (_Fun.AuthType)
            {
                case AuthTypeEnum.Ctrl:
                    //do nothing
                    progList = _Str.ToList(authStrs);
                    break;

                case AuthTypeEnum.Action:
                case AuthTypeEnum.Data:
                    var list = authStrs.Split(',');
                    foreach (var item in list)
                        progList.Add(_Str.GetLeft(item, ":"));
                    break;

                default:
                    return data;
            }

            //read Prog
            var sql = $@"
select Code, Name, Url, Sort
from dbo.XpProg
where Code in ({_List.ToStr(progList, true)})
order by Sort
";
            return await _Db.GetModelsAsync<MenuDto>(sql);
        }

        /// <summary>
        /// convert string to image for front reptcha
        /// width=90 is for 6 char
        /// </summary>
        /// <param name="code"></param>
        /// <param name="fontSize"></param>
        public static void OutputStrImage(string code, int fontSize = 16)
        {
            if (_Str.IsEmpty(code))
                return;

            // System.Drawing.Bitmap image = new System.Drawing.Bitmap((int)Math.Ceiling((checkCode.Length * 12.5)), 22);
            // System.Drawing.Bitmap image = new System.Drawing.Bitmap((int)Math.Ceiling((checkCode.Length * 20)), 40);
            Bitmap image = new(code.Length * 18, 30);
            Graphics g = Graphics.FromImage(image);
            try
            {
                //get random
                Random random = new();

                //reset background color
                g.Clear(Color.White);

                //draw backgroup line for interrupt
                for (int i = 0; (i <= 24); i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }

                Font font = new("Arial", fontSize, (FontStyle.Bold | FontStyle.Italic));
                LinearGradientBrush brush = new(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.DarkRed, 1.2F, true);
                // g.DrawString(checkCode, font, brush, 2, 2);
                g.DrawString(code, font, brush, 2, 2);

                //draw point for interrupt
                for (int i = 0; (i <= 499); i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);
                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }

                //draw border
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, (image.Width - 1), (image.Height - 1));
                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Gif);

                var response = _Http.GetResponse();
                //return image;
                response.Clear();
                response.ContentType = "image/Gif";
                //TODO: response.WriteAsync(ms.ToArray());
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }

    }//class
    #pragma warning restore CA2211 // 非常數欄位不應可見
}

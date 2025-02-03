using Base.Models;
using Base.Services;
using Microsoft.AspNetCore.Http;
using SkiaSharp;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    public static class _Http
    {
        /*
        //private static HttpContext _http;
        //constructor, not work this way !!
        static _Http()
        {
            var service = (IHttpContextAccessor)_Fun.DiBox.GetService(typeof(IHttpContextAccessor));
            _http = service.HttpContext;
        }
        */

        public static HttpContext GetHttp()
        {
            var service = (IHttpContextAccessor)_Fun.DiBox!.GetService(typeof(IHttpContextAccessor))!;
            return service.HttpContext;
        }

        public static string GetIp(bool hasDot = true)
        {
            var ip = GetHttp().Connection.RemoteIpAddress.ToString();
            return hasDot
                ? ip
                : ip.Replace(".", "");
        }

        public static string GetContentTypeByExt(string ext)
        {
            //var ext = _File.GetFileExt(path);
            return (ext == "doc" || ext == "docx") ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document" :
                (ext == "xls" || ext == "xlsx") ? "application/ms-excel" :
                (ext == "pdf") ? "application/pdf" :
                "text/plain";
        }

        //get http request
        public static HttpRequest GetRequest()
        {
            return GetHttp().Request;
        }

        public static string GetCookie(string key)
        {
            return GetHttp().Request.Cookies.TryGetValue(key, out string value)
                ? value : "";
        }

        /// <summary>
        /// cookie有效期限預設7天, 所以不必特別限定
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetCookie(string key, string value)
        {
            GetHttp().Response.Cookies.Append(key, value, new CookieOptions
            {
                Expires = DateTime.UtcNow.AddMinutes(_Fun.TimeOut),
                HttpOnly = true,                      // 避免 JavaScript 存取此 Cookie
                //Secure = true,                        // 僅透過 HTTPS 傳輸（建議）
                //SameSite = SameSiteMode.Strict        // 嚴格限制跨站請求攜帶 Cookie
            });
        }

        /// <summary>
        /// remove cookie
        /// </summary>
        /// <param name="key"></param>
        public static void DeleteCookie(string key)
        {
            GetHttp().Response.Cookies.Delete(key);
        }

        //get http response
        public static HttpResponse GetResponse()
        {
            return GetHttp().Response;
        }

        public static ISession GetSession()
        {
            return GetHttp().Session;
        }

        /*
        public static string GetIp()
        {
            return _Fun.IsDev
                ? ":::1" 
                : GetHttp().Connection.RemoteIpAddress.ToString();
        }
        */

        //get web root path
        public static string GetWebUrl()
        {
            var req = GetRequest();
            return $"{req.Scheme}://{req.Host}{req.Path}{req.QueryString}";
        }

        public static string GetWebPath()
        {
            var req = GetRequest();
            return $"{req.Path}{req.QueryString}";
        }

        /// <summary>
        /// get url result
        /// </summary>
        /// <param name="url"></param>
        /// <param name="args"></param>
        /// <param name="isGet"></param>
        /// <returns></returns>
        public static async Task<string> GetUrlResultA(string url, string args = "", bool isGet = true)
        {
            //TODO
            var request = WebRequest.Create(url) as HttpWebRequest;
            request!.Method = isGet ? "GET" : "POST";
            request.KeepAlive = true; //keep alive
            request.ContentType = "application/x-www-form-urlencoded";

            if (!isGet)
            {
                var bs = Encoding.ASCII.GetBytes(args);
                using var reqStream = request.GetRequestStream();
                await reqStream.WriteAsync(bs.AsMemory(0, bs.Length));
            }

            var response = request.GetResponse() as HttpWebResponse;
            var stream = new StreamReader(response!.GetResponseStream());
            var result = await stream.ReadToEndAsync();
            //stream.Close();
            stream.Dispose();
            return result;
        }

        /// <summary>
        /// get JWT and read BR
        /// ajax呼叫時從http header讀取jwt
        /// form submit時(模擬form submit)從hidden欄位_jwtToken讀取jwt
        /// </summary>
        /// <returns></returns>
        public static BaseUserDto JwtToBr()
        {
            //for get userId, get jwt from http header first
            //var request = GetRequest();
            var token = GetRequest().Headers["Authorization"]
                .ToString().Replace("Bearer ", "");

            /*
            //如果不存在則從 hidden 欄位_jwtToken讀取
            if (token == "" && IsPost() && request.Form != null)
                token = request.Form["_jwtToken"].ToString();
            */

            //re-check 
            if (token == "") return new ();

            var tokenDto = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var userId = tokenDto.Claims.First(c => c.Type == ClaimTypes.Name).Value;  //is also session key
            var br = _Cache.GetModel<BaseUserDto>(UserKeyIp(userId), _Fun.FidBaseUser)!;
            return br;
        }

        /// <summary>
        /// 前端(ex:手機)使用JWT
        /// get userId from JWT string
        /// </summary>
        /// <returns></returns>
        public static string JwtToUserId()
        {
            //for get userId, get jwt from http header first
            //var request = GetRequest();
            var token = GetRequest().Headers["Authorization"]
                .ToString().Replace("Bearer ", "");

            //re-check 
            if (token == "") return "";

            var tokenDto = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return tokenDto.Claims.First(c => c.Type == ClaimTypes.Name).Value;  //is also session key
        }

        /// <summary>
        /// 前端(ex:browser)使用cookie, cookie to BR
        /// 因為網頁重整無法重送JWT, 所以網頁必須使用cookie !!
        /// </summary>
        /// <returns></returns>
        public static BaseUserDto CookieToBr()
        {
            var clientKey = GetCookie(_Fun.FidClientKey);
            return (clientKey == "")
                ? new()
                : _Cache.GetModel<BaseUserDto>(UserKeyIp(clientKey), _Fun.FidBaseUser)!;
        }

        /// <summary>
        /// user key + IP for session
        /// </summary>
        /// <param name="clientKey"></param>
        /// <returns></returns>
        public static string UserKeyIp(string clientKey)
        {
            return clientKey + GetIp(false);
        }

        /// <summary>
        /// is post or not
        /// </summary>
        /// <returns></returns>
        public static bool IsPost()
        {
            return _Http.GetRequest().Method.Equals("POST", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// convert string to image for front captcha
        /// width=90 is for 6 char
        /// </summary>
        /// <param name="code"></param>
        /// <param name="width">about code.Length * 16</param>
        /// <param name="height"></param>
        /// <param name="fontSize"></param>
        public static MemoryStream OutputStrImage(string code, int width = 0, int height = 32, int fontSize = 20)
        {
            if (string.IsNullOrEmpty(code)) code = "??";

            if (width == 0) width = code.Length * 16;
            using var image = new SKBitmap(width, height);
            using var canvas = new SKCanvas(image);
            // Get random
            var random = new Random();

            // Reset background color
            canvas.Clear(SKColors.White);

            // Draw background lines for interruption
            using (var linePaint = new SKPaint { Color = SKColors.Silver })
            {
                for (int i = 0; i <= 24; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    canvas.DrawLine(x1, y1, x2, y2, linePaint);
                }
            }

            using (var textPaint = new SKPaint { TextSize = fontSize, IsAntialias = true, Color = SKColors.Blue })
            {
                // Draw the text
                canvas.DrawText(code, 5, 3 + fontSize, textPaint);
            }

            // Draw points for interruption
            for (int i = 0; i <= 499; i++)
            {
                int x = random.Next(image.Width);
                int y = random.Next(image.Height);
                image.SetPixel(x, y, new SKColor((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)));
            }

            // Draw border
            using (var borderPaint = new SKPaint { Color = SKColors.Silver, IsStroke = true })
            {
                canvas.DrawRect(0, 0, image.Width - 1, image.Height - 1, borderPaint);
            }

            // Save the image to a memory stream
            using MemoryStream ms = new MemoryStream();
            image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);

            return ms;
            /*
            //response image
            var imageData = ms.ToArray();
            var response = GetResponse();
            response.Clear();
            response.ContentType = "image/png";
            await response.Body.WriteAsync(imageData);
            */
        }

        #region remark code
        /// <summary>
        /// sync call remote service
        /// </summary>
        /// <param name="url"></param>
        /// <param name="isPost"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        /*
        public static string Sync(string url, bool isPost = true, string arg = "")
        {
            //set request
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = isPost ? "POST" : "GET";
            request.ContentType = "application/x-www-form-urlencoded";

            //put args into stream
            if (arg != "")
            {
                var bytes = Encoding.UTF8.GetBytes(arg);
                var byteLen = bytes.Length;
                request.ContentLength = byteLen;

                //Write the data to the request stream.  
                var stream = request.GetRequestStream();
                stream.Write(bytes, 0, byteLen);
                stream.Close();
            }

            //get result
            //var result = "";
            try
            {
                //Get the response.  
                var response = request.GetResponse();
                var stream = response.GetResponseStream();
                //Console.WriteLine((response as HttpWebResponse).StatusDescription);

                //Read the content.  
                var reader = new StreamReader(stream);
                var result = reader.ReadToEnd();
                if (result == "{}")
                    result = "";

                //Clean up the streams.  
                reader.Close();
                stream.Close();
                response.Close();

                return result.Trim();    //remove tail carrier !!
            }
            catch (Exception ex)
            {
                _Log.Error("_Http.cs Sync() failed: " + ex.Message);
                return "";
            }
        }
        */

        /*
        public static void ExportExcel(MemoryStream stream, string fileName)
        {
            //return File(stream.ToArray(), "application/vnd.ms-excel", string.Format("{0}.xlsx", fileName));
            HttpResponse response = HttpContext.Current.Response;
            response.ContentType = "application/vnd.ms-excel";
            response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}.xls", fileName));
            response.Clear();

            response.BinaryWrite(stream.GetBuffer());
            response.End();
        }
        */

        /*
        public static void CrudToExcel(ReadModel crud, JObject findJson, string sheetName, List<string> headers = null, List<string> cols = null)
        {
            RowsToExcel(new CrudRead().GetAllRows(crud, findJson), sheetName, headers, cols);
        }

        //把json資料列輸出成為excel檔案, 使用NPOI
        //cols: 欄位id, 如果為null, 則寫全rows全部欄位
        public static void RowsToExcel(JArray rows, string sheetName, List<string> headers = null, List<string> cols = null)
        //public static void JsonsToExcel(string sheetName, JArray rows, List<string> headers = null, List<string> cols = null)
        {
            //var rows = new CrudRead().GetAllRows(crud, findJson);
            //return _Excel.JsonsToExcel(sheetName, rows, headers, cols);

            //set cols & headers
            var rowCount = (rows == null) ? 0 : rows.Count;
            if ((cols == null || cols.Count == 0) && rowCount > 0)
            {
                cols = new List<string>();
                foreach (var item in (JObject)rows[0])
                    cols.Add(item.Key);
            }
            if (headers == null)
                headers = cols;

            //create excel book & worksheet
            IWorkbook book = new XSSFWorkbook(); //XSSF 用來產生Excel 2007檔案（.xlsx）
            ISheet isheet = book.CreateSheet(sheetName);

            //add header
            var excelRow = isheet.CreateRow(0);
            var headerCount = headers.Count;
            for (var i = 0; i < headerCount; i++)
                excelRow.CreateCell(i).SetCellValue(headers[i]);

            //資料寫入excel
            var colCount = (cols == null) ? 0 : cols.Count;
            if (colCount > headerCount)
                colCount = headerCount;
            for (var rowNo = 0; rowNo < rowCount; rowNo++)
            {
                var row = (JObject)rows[rowNo];
                excelRow = isheet.CreateRow(rowNo + 1);
                for (var i = 0; i < colCount; i++)
                    excelRow.CreateCell(i).SetCellValue(row[cols[i]] == null ? "" : row[cols[i]].ToString());
            }

            var mem = new MemoryStream();
            book.Write(mem);

            HttpResponse response = HttpContext.Current.Response;

            //考慮 IE
            //response.AddHeader("Content-Disposition", "attachment; filename=" + sheetName + ".xlsx");
            var browser = HttpContext.Current.Request.Browser;
            if (browser != null && browser.Browser.Equals("ie", StringComparison.OrdinalIgnoreCase))
                response.AppendHeader("Content-Disposition", "attachment; filename*=UTF-8''" + HttpUtility.UrlPathEncode(sheetName) + ".xlsx\"");
            else
                response.AppendHeader("Content-Disposition", "attachment; filename=\"" + HttpUtility.UrlPathEncode(sheetName) + ".xlsx\"");

            response.BinaryWrite(mem.ToArray());
            //response.End();

            //release
            book = null;    
            mem.Close();
            mem.Dispose();
        }
        */

        /*
        //http://coding.anyun.tw/2012/03/14/asp-net-mvc-using-oauth-2-0-connect-google-api/
        public static string GetUrlResult(string url, string args = "", bool isGet = true)
        {
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.Method = isGet ? "GET" : "POST";
            request.KeepAlive = true; //是否保持連線
            request.ContentType = "application/x-www-form-urlencoded";

            var result = "";
            byte[] bs = Encoding.ASCII.GetBytes(args);
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }

            using (WebResponse response = request.GetResponse())
            {
                StreamReader sr = new StreamReader(response.GetResponseStream());
                result = sr.ReadToEnd();
                sr.Close();
            }

            return result;
        }
        */

        /*
        //sync call
        //return result string
        public static string Sync(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            return Sync(request);
        }
        */

        #endregion

    }//class
}

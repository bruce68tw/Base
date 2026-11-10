using Base.Models;
using Base.Services;
using Microsoft.AspNetCore.Http;
using SkiaSharp;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BaseApi.Services
{
    /// <summary>
    /// 提供目前 HTTP 請求與回應的存取、對外 HTTP 呼叫及使用者資料讀取。JWT 讀取方法只解析權杖內容。
    /// </summary>
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

        /// <summary>
        /// 請求帶有非空的 X-PJAX 標頭時回傳 null，否則回傳共用版面路徑。
        /// </summary>
        public static string? PjaxToLayout()
        {
            var isPjax = _Str.NotEmpty(GetRequest().Headers["X-PJAX"]);
            return isPjax
                ? null
            : "~/Views/Shared/_Layout.cshtml";
        }

        /// <summary>
        /// 將字串轉成 URL 編碼。
        /// </summary>
        /// <param name="data">要進行 URL 編碼的字串。</param>
        public static string UrlEncode(string data)
        {
            return HttpUtility.UrlEncode(data);
        }
        /// <summary>
        /// 將 URL 編碼還原為字串。
        /// </summary>
        /// <param name="data">要還原的 URL 編碼字串。</param>
        public static string UrlDecode(string data)
        {
            return HttpUtility.UrlDecode(data);
        }


        /// <summary>
        /// 透過應用 DI 註冊的 IHttpContextAccessor 取得目前 HTTP 請求的內容。
        /// </summary>
        public static HttpContext GetHttp()
        {
            var service = (IHttpContextAccessor)_Fun.DiBox!.GetService(typeof(IHttpContextAccessor))!;
            return service.HttpContext!;
        }

        /// <summary>
        /// 取得目前連線的 RemoteIpAddress。hasDot 為 false 時移除字串中的句點。
        /// </summary>
        /// <param name="hasDot">是否保留 IP 字串中的句點。</param>
        public static string GetIp(bool hasDot = true)
        {
            var ip = GetHttp().Connection.RemoteIpAddress!.ToString();
            return hasDot
                ? ip
                : ip.Replace(".", "");
        }

        /// <summary>
        /// 依副檔名取得 HTTP 內容類型。
        /// 依內建清單對應副檔名，允許前置句點並忽略英文字母大小寫。
        /// </summary>
        /// <param name="ext">要對應內容類型的副檔名，可帶前置句點。</param>
        /// <param name="defaultText">副檔名不在清單時，是否回傳 text/plain。否則回傳空字串。</param>
        public static string ExtToContentType(string ext, bool defaultText)
        {
            ext = ext.ToLower().TrimStart('.');
            return ext switch
            {
                // pdf
                "pdf" => "application/pdf",

                // word
                "doc" => "application/msword",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

                // excel
                "xls" => "application/vnd.ms-excel",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",

                // ppt
                "ppt" => "application/vnd.ms-powerpoint",
                "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",

                // image
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "gif" => "image/gif",
                "bmp" => "image/bmp",
                "webp" => "image/webp",
                "svg" => "image/svg+xml",
                "tif" or "tiff" => "image/tiff",    //可能不適合直接開啟!!

                // zip
                "zip" => "application/zip",

                // text
                "txt" or "log" or "json" or "xml" => "text/plain",

                // fallback
                _ => defaultText ? "text/plain" : ""
            };
        }

        //讀取目前請求。
        /// <summary>
        /// 取得目前 HTTP 請求。
        /// </summary>
        public static HttpRequest GetRequest()
        {
            return GetHttp().Request;
        }

        /// <summary>
        /// 讀取請求 Cookie 的值。鍵不存在時回傳空字串。
        /// </summary>
        /// <param name="key">要從請求讀取的 Cookie 名稱。</param>
        public static string GetCookie(string key)
        {
            return GetHttp().Request.Cookies.TryGetValue(key, out string value)
                ? value : "";
        }

        /// <summary>
        /// 在目前回應設定 Secure、HttpOnly 與 SameSite Strict 的 Cookie，期限依應用的逾時設定。
        /// </summary>
        /// <param name="key">要寫入的 Cookie 名稱。</param>
        /// <param name="value">要寫入的 Cookie 值。</param>
        public static void SetCookie(string key, string value)
        {
            GetHttp().Response.Cookies.Append(key, value, new CookieOptions
            {
                Expires = DateTime.UtcNow.AddMinutes(_Fun.TimeOut), //與server session一致
                HttpOnly = true,                      //避免 JavaScript 存取此 Cookie
                Secure = true,                        //僅透過 HTTPS 傳輸（建議）
                SameSite = SameSiteMode.Strict        //嚴格限制跨站請求攜帶 Cookie
            });
        }

        /// <summary>
        /// 在目前 HTTP 回應加入指定 Cookie 的刪除指示。
        /// </summary>
        /// <param name="key">要在回應中要求刪除的 Cookie 名稱。</param>
        public static void DeleteCookie(string key)
        {
            GetHttp().Response.Cookies.Delete(key);
        }

        //讀取目前回應。
        /// <summary>
        /// 取得目前 HTTP 回應。
        /// </summary>
        public static HttpResponse GetResponse()
        {
            return GetHttp().Response;
        }

        //program.cs必須啟用Session
        /// <summary>
        /// 取得目前請求的 Session，需要應用已啟用 Session middleware。
        /// </summary>
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

        //組合目前請求的完整網址。
        /// <summary>
        /// 組合目前請求的通訊協定、主機、路徑及查詢字串。
        /// </summary>
        public static string GetWebUrl()
        {
            var req = GetRequest();
            return $"{req.Scheme}://{req.Host}{req.Path}{req.QueryString}";
        }

        /// <summary>
        /// 回傳目前請求的路徑及查詢字串。
        /// </summary>
        public static string GetWebPath()
        {
            var req = GetRequest();
            return $"{req.Path}{req.QueryString}";
        }

        /* old syntax
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
        */

        /// <summary>
        /// GET 使用原網址。POST 以 ASCII 表單內容傳送 args。HTTP 非成功狀態會擲出例外。
        /// </summary>
        /// <param name="url">對外請求的網址。GET 的查詢字串需已包含在網址中。</param>
        /// <param name="args">POST 的表單編碼內容，GET 時不使用。</param>
        /// <param name="isGet">為 true 時送出 GET，為 false 時送出 POST。</param>
        public static async Task<string> GetUrlResultA(string url, string args = "", bool isGet = true)
        {
            HttpResponseMessage response;

            if (isGet)
            {
                response = await new HttpClient().GetAsync(url);
            }
            else
            {
                var content = new StringContent(
                    args,
                    Encoding.ASCII,
                    "application/x-www-form-urlencoded");

                response = await new HttpClient().PostAsync(url, content);
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 依 JWT 的使用者識別值取得目前使用者資料。
        /// 現行來源為傳入的權杖或 Authorization 標頭。表單欄位讀取目前停用。只解析權杖並以使用者 ID 查詢資料，簽章和期限由驗證流程處理。
        /// </summary>
        /// <param name="token">要解析的 JWT 原始字串。空字串時從目前請求的 Authorization 標頭取得。</param>
        public static BaseUserDto JwtToBr(string token = "")
        {
            //for get userId, get jwt from http header first
            //var request = GetRequest();
            if (token == "")
            {
                token = GetRequest().Headers["Authorization"]
                    .ToString().Replace("Bearer ", "");
            }

            /*
            //如果不存在則從 hidden 欄位_jwtToken讀取
            if (token == "" && IsPost() && request.Form != null)
                token = request.Form["_jwtToken"].ToString();
            */

            //re-check 
            if (token == "") return new ();

            var tokenDto = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var userId = tokenDto.Claims.First(c => c.Type == ClaimTypes.Name).Value;  //is also session key
            return GetBaseUser(userId);
            //var br = _Cache.GetModel<BaseUserDto>(UserKeyIp(userId), _Fun.FidBaseUser)!;
            //return br;
        }

        /// <summary>
        /// 讀取 BaseUser 資訊, 因為用到 HttpContext, 所以寫在 _Http
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private static BaseUserDto GetBaseUser(string userId)
        {
            //get from HttpContext Itemss if any, 它是只存活在單一 HTTP 請求內的字典
            //如果同一個request已經讀取 BaseUser, 則直接從 Items 讀取, 不再從 cache讀取
            var fid = _Fun.FidBaseUser;
            var items = GetHttp().Items;
            if (items.ContainsKey(fid))
                return (items[fid] as BaseUserDto)!;

            //get from cache
            var br = _Cache.GetModel<BaseUserDto>(UserKeyIp(userId), fid)!;

            //set items
            items[fid] = br;
            return br;
        }

        /// <summary>
        /// 從 Authorization 標頭解析使用者 ID，權杖缺少時回傳空字串。簽章和期限由驗證流程處理。
        /// </summary>
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
        /// 從 Cookie 取得目前使用者資料。
        /// 以用戶端鍵查詢目前使用者資料。Cookie 缺少時回傳新建的空白使用者資料物件。
        /// </summary>
        public static BaseUserDto CookieToBr()
        {
            var clientKey = GetCookie(_Fun.FidClientKey);
            return (clientKey == "")
            ? new()
                : GetBaseUser(clientKey);
                //: _Cache.GetModel<BaseUserDto>(UserKeyIp(clientKey), _Fun.FidBaseUser)!;
        }

        /// <summary>
        /// 直接串接用戶端鍵與移除句點後的連線 IP。
        /// </summary>
        /// <param name="clientKey">要和目前連線 IP 串接的用戶端鍵。</param>
        public static string UserKeyIp(string clientKey)
        {
            return clientKey + GetIp(false);
        }

        /// <summary>
        /// 判斷目前 HTTP 請求是否使用 POST。
        /// </summary>
        public static bool IsPost()
        {
            return _Http.GetRequest().Method.Equals("POST", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 產生供前端驗證碼使用的圖片。
        /// 將文字繪製為 PNG 並回傳記憶體串流。
        /// </summary>
        /// <param name="code">要繪製在圖片上的文字。</param>
        /// <param name="width">圖片寬度，單位為像素。為零時使用文字長度乘以 16。</param>
        /// <param name="height">圖片高度，單位為像素。</param>
        /// <param name="fontSize">繪製文字使用的字型大小。</param>
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

            /* old syntax
            using (var textPaint = new SKPaint { TextSize = fontSize, IsAntialias = true, Color = SKColors.Blue })
            {
                // Draw the text
                canvas.DrawText(code, 5, 3 + fontSize, textPaint);
            }
            */
            using var paint = new SKPaint
            {
                Color = SKColors.Blue,
                IsAntialias = true
            };
            using var font = new SKFont
            {
                Size = fontSize
            };
            canvas.DrawText(code, 5, 3 + fontSize, font, paint);

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
            using var ms = new MemoryStream();
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

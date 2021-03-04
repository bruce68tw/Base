using Base.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BaseWeb.Services
{
    public static class _Http
    {
        /// <summary>
        /// save upload file(HttpPostedFileBase)
        /// </summary>
        /// <param name="dir">save dir path</param>
        /// <param name="file"></param>
        /// <param name="savePath">return file http path</param>
        /// <returns></returns>
        public static async Task<string> SaveFileAsync(string dir, IFormFile file)
        {
            try
            {
                if (file.Length > 0)
                {
                    var savePath = Path.Combine(dir, Path.GetFileName(file.FileName)); //硬碟路徑
                    if (File.Exists(savePath))
                    {
                        var fileName = _Guid.Encode(new Guid()) + Path.GetExtension(file.FileName);
                        dir = Path.Combine(dir, fileName);
                    }
                    //file.SaveAs(savePath);
                    await _WebFile.SaveFileAsync(file, savePath);

                    savePath = dir + Path.GetFileName(file.FileName); //http path
                    return savePath;
                }
                else
                {
                    return "";
                }                    
            }
            catch
            {
                throw;
            }
        }

        //convert string to image
        //output image string to frontEnd for reptcha
        //width=90 is for 6 char
        public static void OutputStrImage(string code, int fontSize = 16)
        {
            if (string.IsNullOrEmpty(code))
                return;

            // System.Drawing.Bitmap image = new System.Drawing.Bitmap((int)Math.Ceiling((checkCode.Length * 12.5)), 22);
            // System.Drawing.Bitmap image = new System.Drawing.Bitmap((int)Math.Ceiling((checkCode.Length * 20)), 40);
            Bitmap image = new Bitmap(code.Length * 18, 30);
            Graphics g = Graphics.FromImage(image);
            try
            {
                //get random
                Random random = new Random();

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

                Font font = new Font("Arial", fontSize, (FontStyle.Bold | FontStyle.Italic));
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.DarkRed, 1.2F, true);
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
                MemoryStream ms = new MemoryStream();
                image.Save(ms, ImageFormat.Gif);

                var response = _Web.GetResponse();
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

        //get web root path
        public static string GetWebUrl()
        {
            var req = _Web.GetRequest();
            return $"{req.Scheme}://{req.Host}{req.Path}{req.QueryString}";
        }

        //?? -> Sync()
        public static string GetUrlResult(string url, string args = "", bool isGet = true)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = isGet ? "GET" : "POST";
            request.KeepAlive = true; //keep alive
            request.ContentType = "application/x-www-form-urlencoded";

            if (!isGet)
            {
                var bs = Encoding.ASCII.GetBytes(args);
                using (var reqStream = request.GetRequestStream())
                {
                    reqStream.Write(bs, 0, bs.Length);
                }
            }

            var response = request.GetResponse() as HttpWebResponse; ;
            var stream = new StreamReader(response.GetResponseStream());
            var result = stream.ReadToEnd();
            //stream.Close();
            stream.Dispose();
            return result;
        }

        //sync call remote service
        //public static string Sync(HttpWebRequest request, byte[] byteArray)
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

        #region remark code
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

using Base.Models;
using Base.Services;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Web;


namespace BaseWeb.Services
{
    public static class _WebExcel
    {
        /// <summary>
        /// excel匯入table, http file stream 會自動關閉, 無法讀取第2次
        /// </summary>
        /// <param name="file">檔案路徑</param>
        /// <param name="sql">insert sql</param>
        /// <param name="excelCols">要匯入的excel檔案的欄位, base 0(配合 excel)</param>
        /// <param name="formats"></param>
        /// <param name="excelStartRow">開始匯入的 row no, base 1(配合 excel)</param>
        /// <param name="db"></param>
        /// <returns></returns>
        //string filePath, string sheetName, int excelStartRow, string toTable, string[] tableCols, bool excelHeader = true, string[] excelCols = null
        public static bool ImportByLargeFile(IFormFile file, string sheetName, int excelStartRow, string toTable, string[] tableCols, string notNullFid = "", bool excelHeader = true, string[] excelCols = null)
        {
            //check
            if (file == null || file.Length == 0)
                return false;

            var filePath = _Fun.DirTemp + file.FileName;
            if (File.Exists(filePath))
                File.Delete(filePath);
            var ok = _Excel.ImportByLargeFile(filePath, sheetName, excelStartRow, toTable, tableCols, notNullFid, excelHeader, excelCols);

            File.Delete(filePath);
            return ok;
        }

        /*
        //string filePath, string insertSql, int[] excelCols, int excelStartRow, bool[] isDates = null, int sheetNo = 0, Db db = null)
        public static bool ImportByFile(HttpPostedFileBase file, string insertSql, int excelStartRow, int[] excelCols, bool[] isDates = null, int sheetNo = 0, Db db = null)
        {
            //check
            if (file == null || file.ContentLength == 0)
                return false;

            return _Excel.ImportByStream(file.InputStream, insertSql, excelStartRow, excelCols, isDates, sheetNo, db);
        }
        */

        public static string ImportByFile<T>(IFormFile file, 
            ExcelImportDto<T> import) where T : class, new()
        {
            //check
            if (file == null || file.Length == 0)
                return "error";

            //TODO pending
            //return new ExcelImportService<T>().ImportByStream(file.InputStream, import);
            return null;
        }

        /// <summary>
        /// 匯出Excel到畫面
        /// </summary>
        /// <param name="crud"></param>
        /// <param name="findJson"></param>
        /// <param name="sheetName"></param>
        /// <param name="headers"></param>
        /// <param name="cols"></param>
        /// <param name="dbStr"></param>
        public static void ScreenByCrud(ReadDto crud, JObject findJson, string fileStem, string template = "", int srcRowNo = 1, string dbStr = "")
        {
            ScreenByRows(new CrudRead(dbStr).GetAllRows(crud, findJson, true), fileStem, template, srcRowNo);
        }

        public static void ScreenBySql(string sql, string fileStem, string template = "", int srcRowNo = 1, string dbStr = "")
        {
            var rows = _Db.GetJsons(sql, null, dbStr);
            ScreenByRows(rows, fileStem, template, srcRowNo);
        }

        /// <summary>
        /// 匯出Excel到畫面
        /// 使用 OpenXml
        /// https://blog.johnwu.cc/article/asp-net-core-export-to-excel.html
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="sheetName"></param>
        /// <param name="headers"></param>
        /// <param name="cols"></param>
        public static void ScreenByRows(JArray rows, string fileStem, string template = "", int srcRowNo = 1)
        {
            /* 
            //TODO: pending
            var ms = new MemoryStream();
            //FileStream fs = null;
            SpreadsheetDocument docx = null;
            var hasTemplate = !string.IsNullOrEmpty(template);
            if (hasTemplate)
            {
                docx = SpreadsheetDocument.CreateFromTemplate(template);                
                _Excel.DocxByRows(rows, docx, srcRowNo);
                docx.Clone(ms);
            }
            else
            {
                docx = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook);
                _Excel.DocxByRows(rows, docx, srcRowNo);
            }
            docx.Dispose();

            //response stream, 考慮 IE
            var response = HttpContext.Current.Response;
            var browser = HttpContext.Current.Request.Browser;
            if (browser != null && browser.Browser.Equals("ie", StringComparison.OrdinalIgnoreCase))
                response.AppendHeader("Content-Disposition", "attachment; filename*=UTF-8''" + HttpUtility.UrlPathEncode(fileStem) + ".xlsx\"");
            else
                response.AppendHeader("Content-Disposition", "attachment; filename=\"" + HttpUtility.UrlPathEncode(fileStem) + ".xlsx\"");

            response.BinaryWrite(ms.ToArray());

            //response.End();

            //release
            //stream.Close();
            ms.Dispose();
            */
        }

        #region remark code
        /*
        public static bool ToTableByStream(Stream stream, string sql, List<int> excelCols, List<string> formats, int excelStartRow, int sheetNo = 0, bool closeBook = true, Db db = null)
        {
            return _Excel.ToTableByStream(stream, sql, excelCols, formats, excelStartRow, sheetNo, closeBook, db);
        }
        */

        /*
        //response excel檔案到前端
        //TODO: 把核心部分移到 _Excel.cs
        //把json資料列輸出成為excel檔案, 使用NPOI(不支援 .net core !!)
        //cols: 欄位id, 如果為null, 則寫全rows全部欄位
        public static void RowsToExcel(JArray rows, string sheetName, List<string> headers = null, List<string> cols = null)
        {
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
        #endregion

    }//class
}

using Base.Models;
using Base.Services;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.IO;


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
        /*
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

        //string filePath, string insertSql, int[] excelCols, int excelStartRow, bool[] isDates = null, int sheetNo = 0, Db db = null)
        public static bool ImportByFile(HttpPostedFileBase file, string insertSql, int excelStartRow, int[] excelCols, bool[] isDates = null, int sheetNo = 0, Db db = null)
        {
            //check
            if (file == null || file.ContentLength == 0)
                return false;

            return _Excel.ImportByStream(file.InputStream, insertSql, excelStartRow, excelCols, isDates, sheetNo, db);
        }
        */

        /// <summary>
        /// import by file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="importType"></param>
        /// <param name="importDto"></param>
        /// <param name="frontDtFormat">skip if excel no datetime cell</param>
        public static ResultImportDto ImportByFile<T>(IFormFile file, 
            ExcelImportDto<T> importDto, string frontDtFormat = "") where T : class, new()
        {
            //check
            if (file == null || file.Length == 0)
                return new ResultImportDto()
                {
                    ErrorMsg = "Upload file is empty.",
                };

            return new ExcelImportService<T>().ImportByStream(file.OpenReadStream(), importDto, file.FileName, frontDtFormat);
        }

        /// <summary>
        /// readDto to screen
        /// </summary>
        /// <param name="readDto"></param>
        /// <param name="findJson"></param>
        /// <param name="fileName"></param>
        /// <param name="tplPath"></param>
        /// <param name="srcRowNo"></param>
        /// <param name="dbStr"></param>
        public static void ExportByRead(ReadDto readDto, JObject findJson, string fileName, string tplPath = "", int srcRowNo = 1)
        {
            ExportByRows(new CrudRead().GetAllRows(readDto, findJson, true), fileName, tplPath, srcRowNo);
        }

        /// <summary>
        /// sql to screen
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="fileName"></param>
        /// <param name="tplPath"></param>
        /// <param name="srcRowNo"></param>
        /// <param name="dbStr"></param>
        public static void ExportBySql(string sql, string fileName, string tplPath = "", int srcRowNo = 1, string dbStr = "")
        {
            var rows = _Db.GetJsons(sql, null, dbStr);
            ExportByRows(rows, fileName, tplPath, srcRowNo);
        }

        /// <summary>
        /// json rows to screen
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="fileName"></param>
        /// <param name="tplPath"></param>
        /// <param name="srcRowNo"></param>
        public static void ExportByRows(JArray rows, string fileName, string tplPath = "", int srcRowNo = 1)
        {
            var ms = new MemoryStream();
            SpreadsheetDocument docx = null;
            if (string.IsNullOrEmpty(tplPath))
            {
                //use using syntax, or excel will open failed !!
                srcRowNo = 0;
                using (docx = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook))
                    _Excel.RowsToDocx(rows, docx, srcRowNo);
            }
            else
            {
                var tplBytes = File.ReadAllBytes(tplPath);
                ms.Write(tplBytes, 0, tplBytes.Length);
                using (docx = SpreadsheetDocument.Open(ms, true))
                    _Excel.RowsToDocx(rows, docx, srcRowNo);
            }
            _Web.StreamToScreen(ms, fileName);
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

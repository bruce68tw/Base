using Base.Models;
using Base.Services;
using BaseApi.Services;
using BaseOffice.Models;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace BaseOffice.Services
{
    /// <summary>提供 Excel 表單上傳匯入及目前 HTTP 回應的下載匯出。</summary>
    public static class _HttpExcel
    {
        /// <summary>從表單上傳檔讀取 Excel，依匯入設定處理並回傳匯入結果。</summary>
        /// <param name="file">前端上傳的 Excel 檔案</param>
        /// <param name="importDto">欄位對應、驗證及資料處理的匯入設定</param>
        /// <param name="uiDtFormat">日期儲存格轉成字串時使用的格式</param>
        /// <param name="writeLog">是否寫入XpImportLog table, 如果要自行控制寫入log, 則設為false</param>
        /// <param name="dirUpload">儲存匯入檔案的目錄</param>
        public static async Task<ResultImportDto> ImportByFileA<T>(IFormFile file, string dirUpload,
            ExcelImportDto<T> importDto, string uiDtFormat = "", bool writeLog = true) where T : class, new()
        {
            //check
            if (file == null || file.Length == 0)
                return new ResultImportDto()
                {
                    _ErrorMsg = "Upload file is empty.",
                };

            // 每個上傳檔建立獨立的匯入服務，讓成功列與失敗列的紀錄只屬於這次匯入。
            return await new ExcelImportSvc<T>().ImportByStreamA(file.OpenReadStream(), importDto, dirUpload, file.FileName, uiDtFormat, writeLog);
        }

        /// <summary>依 CRUD 查詢與匯出權限取得資料，填入 Excel 範本後回傳下載。</summary>
        /// <param name="ctrl">對應作業的控制器名稱</param>
        /// <param name="readDto">查詢 SQL 與欄位條件設定</param>
        /// <param name="findJson">查詢欄位與條件值的 JSON</param>
        /// <param name="fileName">檔案名稱，必要時包含副檔名</param>
        /// <param name="tplPath">要讀取的範本檔案路徑</param>
        /// <param name="srcRowNo">資料列寫入 SheetData 的零起算插入位置。</param>
        public static async Task ExportByReadA(string ctrl, ReadDto readDto, JObject findJson, 
            string fileName, string tplPath, int srcRowNo)
        {
            var rows = await new CrudReadSvc().GetExportRowsA(ctrl, readDto, findJson);
            if (rows != null)
                await ExportByRowsA(rows, fileName, tplPath, srcRowNo);
        }

        /// <summary>執行 SQL 取得資料，填入 Excel 範本後回傳下載。</summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="fileName">檔案名稱，必要時包含副檔名</param>
        /// <param name="tplPath">要讀取的範本檔案路徑</param>
        /// <param name="srcRowNo">資料列寫入 SheetData 的零起算插入位置。</param>
        public static async Task ExportBySqlA(string sql, string fileName, string tplPath, 
            int srcRowNo)
        {
            var rows = await _Db.GetRowsA(sql);
            if (rows != null)
                await ExportByRowsA(rows, fileName, tplPath, srcRowNo);
        }

        /// <summary>將指定 JSON 資料列填入 Excel 範本並寫入目前 HTTP 回應。</summary>
        /// <param name="rows">依順序處理的資料列集合</param>
        /// <param name="fileName">檔案名稱，必要時包含副檔名</param>
        /// <param name="tplPath">要讀取的範本檔案路徑</param>
        /// <param name="srcRowNo">資料列寫入 SheetData 的零起算插入位置。</param>
        public static async Task ExportByRowsA(JArray rows, string fileName, string tplPath, int srcRowNo)
        {
            // 讀取檔案 bytes
            var tplBytes = File.ReadAllBytes(tplPath);

            // 將 bytes 寫入 MemoryStream
            var ms = new MemoryStream();
            ms.Write(tplBytes, 0, tplBytes.Length);
            ms.Position = 0;  // 一定要重置位置

            // 讀取 Excel 物件
            var docx = SpreadsheetDocument.Open(ms, true);

            _Excel.DocxByRows(rows, docx, srcRowNo);
            docx.Dispose(); //must dispose, or get empty excel !!
            //ms.Position = 0;
            // 匯出工具會將串流移回開頭，再寫入目前的 HTTP 回應。
            await _Fun2.ExportByStreamA(ms, fileName);
        }

        #region remark code
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
        public static bool ImportByFile(HttpPostedFileBase file, string insertSql, int excelStartRow, int[] excelCols, bool[] isDates = null, int sheetNo = 0, Db? db = null)
        {
            //check
            if (file == null || file.ContentLength == 0)
                return false;

            return _Excel.ImportByStream(file.InputStream, insertSql, excelStartRow, excelCols, isDates, sheetNo, db);
        }
        */
        #endregion

    }//class
}

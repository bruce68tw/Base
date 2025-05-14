﻿using Base.Models;
using Base.Services;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    public static class _HttpExcel
    {
        /// <summary>
        /// import by file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="importType"></param>
        /// <param name="importDto"></param>
        /// <param name="uiDtFormat">skip if excel no datetime cell</param>
        /// <param name="writeLog">是否寫入XpImportLog table, 如果要自行控制寫入log, 則設為false</param>
        public static async Task<ResultImportDto> ImportByFileA<T>(IFormFile file, string dirUpload,
            ExcelImportDto<T> importDto, string uiDtFormat = "", bool writeLog = true) where T : class, new()
        {
            //check
            if (file == null || file.Length == 0)
                return new ResultImportDto()
                {
                    ErrorMsg = "Upload file is empty.",
                };

            return await new ExcelImportSvc<T>().ImportByStreamA(file.OpenReadStream(), importDto, dirUpload, file.FileName, uiDtFormat, writeLog);
        }

        /// <summary>
        /// readDto to screen
        /// </summary>
        /// <param name="ctrl">controller name</param>
        /// <param name="readDto"></param>
        /// <param name="findJson"></param>
        /// <param name="fileName">output fileName</param>
        /// <param name="tplPath"></param>
        /// <param name="srcRowNo"></param>
        public static async Task ExportByReadA(string ctrl, ReadDto readDto, JObject findJson, 
            string fileName, string tplPath, int srcRowNo)
        {
            var rows = await new CrudReadSvc().GetExportRowsA(ctrl, readDto, findJson);
            if (rows != null)
                await ExportByRowsA(rows, fileName, tplPath, srcRowNo);
        }

        /// <summary>
        /// sql to screen
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="fileName">output fileName</param>
        /// <param name="tplPath"></param>
        /// <param name="srcRowNo"></param>
        /// <param name="dbStr"></param>
        public static async Task ExportBySqlA(string sql, string fileName, string tplPath, 
            int srcRowNo)
        {
            var rows = await _Db.GetRowsA(sql);
            if (rows != null)
                await ExportByRowsA(rows, fileName, tplPath, srcRowNo);
        }

        /// <summary>
        /// json rows to screen
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="fileName">output fileName</param>
        /// <param name="tplPath"></param>
        /// <param name="srcRowNo"></param>
        public static async Task ExportByRowsA(JArray rows, string fileName, string tplPath, int srcRowNo)
        {
            var ms = new MemoryStream();
            var docx = _Excel.FileToMsDocx(tplPath, ms);
            _Excel.DocxByRows(rows, docx, srcRowNo);
            docx.Dispose(); //must dispose, or get empty excel !!
            //ms.Position = 0;
            await _FunApi.ExportByStreamA(ms, fileName);
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

using Base.Services;
using System.Collections.Generic;

namespace Base.Models
{
    //匯入excel時的欄位屬性設定
    //來源的excel檔名為 Id + "_Source.xlsx"
    //匯入失敗的excel檔名為 Id + "_Fail.xlsx"
    public class ExcelImportDto<T> where T : class, new()
    {
        public FnCheckImportRow<T> FnCheckImportRow = null;
        public FnSaveImportRows<T> FnSaveImportRows;

        //public string SaveFilePath;
        //public string ErrorFilePath;
        public string TplFilePath;

        /// <summary>
        /// 如果空白, 表示excel第一列為欄位Id, 欄位順序必須與excel檔案相同
        /// </summary>
        public List<string> ExcelFids = new List<string>();

        /// <summary>
        /// 內容為excel欄位代號, ex:A,B.., 如果不為空白, 則內容/順序必須與excelFids對應
        /// </summary>
        //public List<string> ExcelColNames = new List<string>();

        /// <summary>
        /// base 1, excel資料開始列數, default 2
        /// </summary>
        public int ExcelStartRow = 2;

        /// <summary>
        /// base 0, 要匯入的 excel sheet no, default 0
        /// </summary>
        public int SheetNo = 0;

        public string LogRowId;
        public string SaveDir;
        public string UploadFileName;
        public string CreatorName;

    }
}
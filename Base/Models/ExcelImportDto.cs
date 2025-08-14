using Base.Services;
using System.Collections.Generic;

namespace Base.Models
{
    //for excel import function
    //import source file name: Id + ".xlsx"
    //import failed file name: Id + "_fail.xlsx"
    public class ExcelImportDto<T> where T : class, new()
    {
        //public FnCheckImportRow<T> FnCheckImportRow = null;

        /// <summary>
        /// (delegate) save imported rows into DB 
        /// ex: private List<string> SaveImportRows(List<T> okRows)
        /// </summary>
        public FnSaveImportRows<T>? FnSaveImportRows;

        /// <summary>
        /// map to ImportLog.Type
        /// </summary>
        public string ImportType = "";

        /// <summary>
        /// template file path
        /// </summary>
        public string TplPath = "";

        /// <summary>
        /// directory for save excel file(source & failed)
        /// </summary>
        //public string DirSaveFile;

        /// <summary>
        /// empty means excel row0 is fid, order should same to imported file
        /// c# 讀取 excel 日期欄位會變成數字(從1900/1/1開始計算), 如果<T>的欄位type為string, 但excel欄位為日期, 
        /// 則該欄位應寫為D:xxx, 表示系統會轉成日期字串再儲存
        /// </summary>
        public List<string> ExcelFids = [];

        /// <summary>
        /// excel col name, ex:A,B.., if not empty then array/order should map th ExcelFids
        /// </summary>
        //public List<string> ExcelColNames = new List<string>();

        /// <summary>
        /// excel data sheet no(base 0), default 0
        /// </summary>
        public int SheetNo = 0;

        /// <summary>
        /// excel fid start row(base 1, 與excel相同), default 1
        /// </summary>
        public int FidRowNo = 1;

        /// <summary>
        /// ImportLog table row Id, set to _Str.NewId() if empty
        /// </summary>
        public string LogRowId = "";

        /// <summary>
        /// creator name
        /// </summary>
        public string CreatorName = "";

    }
}
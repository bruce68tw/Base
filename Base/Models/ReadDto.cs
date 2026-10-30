using Base.Services;
using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// CRUD 列表與匯出使用的 SQL 及查詢條件設定。
    /// </summary>
    public class ReadDto
    {
        /// <summary>
        /// 選取欄位清單設定；是否使用依呼叫端。
        /// </summary>
        public string ColList = "";
        /// <summary>
        /// 是否使用 [from]、[where]、[group]、[order] 區段標記。
        /// </summary>
        public bool UseSquare = false;
        /// <summary>
        /// 列表查詢 SQL；選取欄位順序應與前端排序對應。
        /// </summary>
        public string ReadSql = "";
        /// <summary>
        /// 匯出用 SQL；空白時沿用 ReadSql。
        /// </summary>
        public string ExportSql = "";
        /// <summary>
        /// 查詢欄位預設的資料表別名，不含尾端句點。
        /// </summary>
        public string TableAs = "";
        /// <summary>
        /// 個人資料權限的 SQL 條件格式，{0} 帶入使用者 Id。
        /// </summary>
        public string WhereUserFid = _Fun.UserEqual;
        /// <summary>
        /// 部門資料權限的 SQL 條件格式，{0} 帶入部門 Id。
        /// </summary>
        public string WhereDeptFid = _Fun.DeptEqual;
        /// <summary>
        /// 快速搜尋的 SQL 欄位清單，採包含比對。
        /// </summary>
        public string[]? FindCols;
        /// <summary>
        /// 以 OR 合併的查詢欄位群組設定。
        /// </summary>
        public List<List<string>>? OrGroups;
        /// <summary>
        /// 各輸入欄位的查詢規則，可為 null。
        /// </summary>
        public QitemDto[]? Items;

    }//class
}

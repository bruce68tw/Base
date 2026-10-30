
namespace Base.Models
{
    /// <summary>
    /// 拆分後的 SQL 區段，供組合查詢與分頁使用。
    /// </summary>
    public class SqlDto
    {
        /// <summary>
        /// 不含 SELECT 關鍵字的選取片段。
        /// </summary>
        public string Select = "";
        /// <summary>
        /// 選取欄位的拆分清單，順序供排序等處理使用。
        /// </summary>
        public string[] Columns = {};     
        /// <summary>
        /// FROM 區段，包含關鍵字。
        /// </summary>
        public string From = "";
        /// <summary>
        /// WHERE 區段，包含關鍵字。
        /// </summary>
        public string Where = "";
        /// <summary>
        /// GROUP BY 區段，包含關鍵字。
        /// </summary>
        public string Group = "";
        /// <summary>
        /// ORDER BY 區段，包含關鍵字。
        /// </summary>
        public string Order = "";
    }
}

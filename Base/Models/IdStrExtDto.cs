
namespace Base.Models
{
    /// <summary>
    /// 附帶一個擴充字串的選項資料。
    /// </summary>
    public class IdStrExtDto
    {
        /// <summary>
        /// 項目識別碼。
        /// </summary>
        public string Id { get; set; } = "";
        /// <summary>
        /// 項目顯示文字。
        /// </summary>
        public string Str { get; set; } = "";
        /// <summary>
        /// 由查詢或應用指定的擴充值。
        /// </summary>
        public string Ext { get; set; } = "";
    }
}

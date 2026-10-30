
namespace Base.Models
{
    /// <summary>
    /// 附帶兩個擴充字串的選項資料。
    /// </summary>
    public class IdStrExt2Dto
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
        /// 第一個擴充值。
        /// </summary>
        public string Ext { get; set; } = "";
        /// <summary>
        /// 第二個擴充值。
        /// </summary>
        public string Ext2 { get; set; } = "";
    }
}

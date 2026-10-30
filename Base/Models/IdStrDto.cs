namespace Base.Models
{
    /// <summary>
    /// 識別碼與顯示文字的配對，常用於選項清單。
    /// </summary>
    public class IdStrDto
    {
        /// <summary>
        /// 項目識別碼。
        /// </summary>
        public string Id { get; set; } = "";
        /// <summary>
        /// 項目顯示文字。
        /// </summary>
        public string Str { get; set; } = "";
    }
}

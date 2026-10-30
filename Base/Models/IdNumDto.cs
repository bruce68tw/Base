namespace Base.Models
{
    /// <summary>
    /// 字串識別碼搭配整數值的資料項目。
    /// </summary>
    public class IdNumDto
    {
        /// <summary>
        /// 項目識別碼，也可作為統計分類標籤。
        /// </summary>
        public string Id { get; set; } = "";
        /// <summary>
        /// 項目對應的整數值。
        /// </summary>
        public int Num { get; set; }

    }//class
}

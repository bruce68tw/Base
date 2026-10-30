namespace Base.Models
{
    /// <summary>
    /// 以資料分類及列分類表示的整數彙總資料。
    /// </summary>
    public class CountDto
    {
        /// <summary>
        /// 彙總資料的分類。
        /// </summary>
        public string DataType { get; set; } = "";
        /// <summary>
        /// 彙總列的分類。
        /// </summary>
        public string RowType { get; set; } = "";
        /// <summary>
        /// 此分類的整數彙總值。
        /// </summary>
        public int Amount { get; set; }
    }
}

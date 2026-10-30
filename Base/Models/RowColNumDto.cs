
namespace Base.Models
{
    /// <summary>
    /// 分組統計圖使用的列分類、欄分類與整數值。
    /// </summary>
    public class RowColNumDto
    {
        /// <summary>
        /// 資料系列的分類名稱。
        /// </summary>
        public string Row { get; set; } = "";
        /// <summary>
        /// 對應圖表標籤的分類識別。
        /// </summary>
        public string Col { get; set; } = "";
        /// <summary>
        /// 此列欄組合的整數值。
        /// </summary>
        public int Num { get; set; }
    }
}

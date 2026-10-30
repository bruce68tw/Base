namespace Base.Models
{
    /// <summary>
    /// 整數序號與字串值的配對。
    /// </summary>
    public class SnStrDto
    {
        /// <summary>
        /// 項目序號；Excel 匯入錯誤清單用於記錄列號。
        /// </summary>
        public int Sn { get; set; }
        /// <summary>
        /// 對應文字，例如匯入錯誤訊息。
        /// </summary>
        public string Str { get; set; } = "";
    }
}

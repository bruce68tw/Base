namespace Base.Models
{
    /// <summary>
    /// 供畫面顯示的簽核紀錄。
    /// </summary>
    public class SignRowDto
    {
        /// <summary>
        /// 簽核節點名稱。
        /// </summary>
        public string NodeName { get; set; } = "";
        /// <summary>
        /// 簽核者顯示名稱。
        /// </summary>
        public string SignerName { get; set; } = "";
        //public string Locale { get; set; }
        /// <summary>
        /// 簽核狀態的顯示文字，非狀態代碼。
        /// </summary>
        public string SignStatusName { get; set; } = "";
        /// <summary>
        /// 簽核時間的顯示字串。
        /// </summary>
        public string SignTime { get; set; } = "";
        /// <summary>
        /// 簽核意見或備註。
        /// </summary>
        public string Note { get; set; } = "";
    }
}

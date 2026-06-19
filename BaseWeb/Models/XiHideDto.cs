namespace BaseWeb.Models
{
    /// <summary>
    /// XiHide 隱藏文字欄位的傳入參數類型，因為內容較單純沒有繼承 XiBaseDto
    /// </summary>
    public class XiHideDto
    {
        /// <summary>
        /// 同 XiBaseDto
        /// </summary>
        public string Fid { get; set; } = "";

        /// <summary>
        /// 同 XiBaseDto
        /// </summary>
        public string Value { get; set; } = "";

        /// <summary>
        /// 同 XiBaseDto
        /// </summary>
        public string InputAttr { get; set; } = "";

    }
}
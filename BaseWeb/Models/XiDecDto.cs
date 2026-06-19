namespace BaseWeb.Models
{
    /// <summary>
    /// XiDec 數字輸入欄位(有小數)的傳入參數類型
    /// </summary>
    public class XiDecDto : XiBaseDto
    {
        /// <summary>
        /// 同 XiIntDto
        /// </summary>
        public decimal Min { get; set; } = 0;

        /// <summary>
        /// 同 XiIntDto
        /// </summary>
        public decimal Max { get; set; } = 0;

        /// <summary>
        /// 同 XiTextDto
        /// </summary>
        public string FnOnBlur { get; set; } = "";

    }
}
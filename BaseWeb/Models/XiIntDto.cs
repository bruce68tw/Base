namespace BaseWeb.Models
{
    /// <summary>
    /// XiInt 整數輸入欄位的傳入參數類型
    /// </summary>
    public class XiIntDto : XiBaseDto
    {
        /// <summary>
        /// 最小值限制
        /// </summary>
        public int Min { get; set; } = 0;

        /// <summary>
        /// 最大值限制
        /// </summary>
        public int Max { get; set; } = 0;

        /// <summary>
        /// 同 XiTextDto
        /// </summary>
        public string FnOnBlur { get; set; } = "";

    }
}
using Base.Enums;

namespace BaseWeb.Models
{
    /// <summary>
    /// XiTextarea 多行文字輸入欄位的傳入參數類型
    /// </summary>
    public class XiTextareaDto : XiBaseDto
    {
        /// <summary>
        /// 同 XiTextDto
        /// </summary>
        public int MaxLen { get; set; } = 0;

        /// <summary>
        /// 輸入欄位的行數，即為 textarea 的 rows 屬性
        /// </summary>
        public int RowsCount { get; set; } = 3;

        /// <summary>
        /// 同 XiTextDto
        /// </summary>
        public string FnOnBlur { get; set; } = "";

        /// <summary>
        /// 同 XiTextDto
        /// </summary>
        public string Pattern { get; set; } = InputPatternEstr.EngNumExt;
    }
}
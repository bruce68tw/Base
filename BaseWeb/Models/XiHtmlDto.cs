namespace BaseWeb.Models
{
    /// <summary>
    /// XiHtml HTML編輯器欄位的傳入參數類型，使用 SummerNote 套件
    /// </summary>
    public class XiHtmlDto : XiBaseDto
    {
        /// <summary>
        /// 同 XiTextDto
        /// </summary>
        public int MaxLen { get; set; } = 0;
    }
}
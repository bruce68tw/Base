namespace BaseWeb.Models
{
    /// <summary>
    /// XiDt 日期時間(datetime)輸入欄位的傳入參數類型
    /// </summary>
    public class XiDtDto : XiBaseDto
    {
        /// <summary>
        /// 分鐘為下拉式欄位，此欄位表示select options的時間間隔
        /// </summary>
        public int MinuteStep { get; set; } = 10;
    }
}
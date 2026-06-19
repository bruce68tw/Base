namespace BaseWeb.Models
{
    /// <summary>
    /// XiLink 鏈結欄位的傳入參數類型，此為 href 鏈結，此欄位一般用來檢視圖檔或下載檔案
    /// </summary>
    public class XiLinkDto : XiBaseDto
    {
        /// <summary>
        /// 資料表名稱，用來分辨要存取的檔案
        /// </summary>
        public string Table { get; set; } = "";
    }
}
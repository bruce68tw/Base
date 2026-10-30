namespace Base.Models
{
    /// <summary>
    /// 上傳檔案的實體儲存位置與回傳網址前綴。
    /// </summary>
    public class UploadDto
    {
        /// <summary>
        /// 實體根路徑；空白時使用 AppDomain.CurrentDomain.BaseDirectory。
        /// </summary>
        public string ServerPath = "";
        /// <summary>
        /// 相對儲存目錄，與根路徑及檔名組成實體位置。
        /// </summary>
        public string SaveDir = "";
        /// <summary>
        /// 網址前綴，與儲存目錄及檔名組成回傳連結。
        /// </summary>
        public string PreUrl = "";
        
    }
}

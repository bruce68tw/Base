namespace Base.Models
{
    //for 檔案上傳
    public class UploadDto
    {
        /// <summary>
        /// 主機實體路徑 for save檔案, 如果空白則表示目前程式路徑
        /// Path表示實際路徑, url表示連結路徑
        /// </summary>
        public string ServerPath = "";

        /// <summary>
        /// 儲存目錄, 不可空白
        /// ServerPath + SaveDir + 檔案名稱 = 儲存路徑
        /// </summary>
        public string SaveDir = "";

        /// <summary>
        /// 檔案連結前面的字串
        /// PreUrl + SaveDir + 檔案名稱 = 檔案在網頁上的 url
        /// </summary>
        public string PreUrl = "";
        
    }
}
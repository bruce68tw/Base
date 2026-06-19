namespace BaseWeb.Models
{
    /// <summary>
    /// XiRead 唯讀欄位的傳入參數類型
    /// </summary>
    public class XiReadDto : XiBaseDto
    {
        /// <summary>
        /// 資料的顯示格式，通常用在日期/時間欄位
        /// </summary>
        public string Format { get; set; } = "";

        /// <summary>
        /// 是否存入後端資料庫, 唯讀欄位預設不儲存
        /// </summary>
        public bool SaveDb { get; set; } = false;

        /// <summary>
        /// (舊名為 EditStyle)
        /// 是否有外框(與XiText外觀類似)，預設無外框
        /// </summary>
        public bool Border { get; set; } = false;
    }
}
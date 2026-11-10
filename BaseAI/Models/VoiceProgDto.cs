namespace BaseAI.Models
{
    /// <summary>第一階段語音辨識結果，包含逐字稿、動作與功能名稱。</summary>
    /// <remarks>只存資料；功能對應、權限檢查與執行由程式處理。</remarks>
    public class VoiceProgDto
    {
        /// <summary>本次語音中可辨識內容的完整逐字稿。</summary>
        public string VoiceText { get; set; } = "";

        /// <summary>0 表示開啟畫面，1 表示開啟後執行查詢，空字串表示動作未辨識或不支援。</summary>
        public string IsQuery { get; set; } = "";

        /// <summary>辨識出的功能名稱，保留原語言，無法對應時為空字串。</summary>
        public string ProgName { get; set; } = "";
    }

}

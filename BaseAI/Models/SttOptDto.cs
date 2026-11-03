namespace BaseAI.Models
{
    /// <summary>單次轉錄提示；只提供辨識背景，不包含功能執行規則。</summary>
    public class SttOptDto
    {
        /// <summary>精簡的系統或錄音背景，不應要求模型補寫沒有說出的內容。</summary>
        public string Context { get; set; } = "";

        /// <summary>可能出現的專有名詞；僅為提示，不代表這些詞必須出現在逐字稿。</summary>
        public string[] Keywords { get; set; } = [];

        /// <summary>預期語言，例如 zh-TW；空字串由模型判斷。</summary>
        public string Language { get; set; } = "";

        /// <summary>整次 HTTP 要求的上限，須大於零；預設為 20 秒。</summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(20);
    }
}

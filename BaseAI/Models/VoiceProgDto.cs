namespace BaseAI.Models
{
    /// <summary>語音指令，包含動作、功能名稱與參數。</summary>
    /// <remarks>只存資料；功能對應、權限檢查與執行由程式處理。</remarks>
    public class VoiceProgDto
    {
        /// <summary>語音文字。</summary>
        public string VoiceText { get; set; } = "";

        /// <summary>0：打開功能畫面；1: 打開畫面並執行查詢。</summary>
        public string IsQuery { get; set; } = "";

        /// <summary>功能中文名稱，由 AI 查對照表取得。</summary>
        public string ProgName { get; set; } = "";
    }

}

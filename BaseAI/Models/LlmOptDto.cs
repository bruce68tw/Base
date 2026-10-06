namespace BaseAI.Models
{
    /// <summary>
    /// 建立 Live LLM 工作階段時使用的連線、生成與對話設定。
    /// </summary>
    public class LlmOptDto
    {
        /// <summary>Live LLM provider 的 WebSocket 端點。</summary>
        public string EndPoint { get; set; } = "";

        /// <summary>要使用的 Live LLM 模型名稱或識別碼。</summary>
        public string Model { get; set; } = "";

        /// <summary>語音回覆使用的語音名稱。</summary>
        public string VoiceName { get; set; } = "";

        /// <summary>控制模型回覆方式與內容的系統提示詞。</summary>
        public string SystemPrompt { get; set; } = "";

        /// <summary>對話使用的語言設定，預設為繁體中文（zh-TW）；目前由呼叫端提供，provider 尚未直接使用此欄位。</summary>
        public string Language { get; set; } = "zh-TW";

        /// <summary>是否要求模型產生語音回覆；false 時只要求文字回覆。</summary>
        public bool AudioResponse { get; set; } = true;

        /// <summary>提供給模型呼叫的工具宣告集合。</summary>
        public IReadOnlyCollection<LlmToolDto> Tools { get; set; } = Array.Empty<LlmToolDto>();

        /// <summary>建立或重連工作階段時載入的既有對話紀錄。</summary>
        public IReadOnlyCollection<LlmHistoryDto> History { get; set; } = Array.Empty<LlmHistoryDto>();
    }
}

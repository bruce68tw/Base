namespace BaseAI.Models
{
    /// <summary>提供給即時模型的工具宣告。</summary>
    public sealed class LlmToolDto
    {
        /// <summary>供模型呼叫的工具名稱。</summary>
        public string Name { get; set; } = "";
        /// <summary>說明工具的用途，供模型選擇時參考。</summary>
        public string Description { get; set; } = "";
        /// <summary>工具輸入參數的結構定義。</summary>
        public object Parameters { get; set; } = new { };
    }

    /// <summary>模型提出的單筆工具呼叫。</summary>
    public sealed class LlmToolCallDto
    {
        /// <summary>用於對應工具結果的呼叫 ID。</summary>
        public string Id { get; set; } = "";
        /// <summary>模型要求執行的工具名稱。</summary>
        public string Name { get; set; } = "";
        /// <summary>模型提供的工具參數 JSON。</summary>
        public string ArgumentsJson { get; set; } = "{}";
    }

    /// <summary>應用程式執行工具後回送模型的結果。</summary>
    public sealed class LlmToolRespDto
    {
        /// <summary>對應原始工具呼叫的 ID。</summary>
        public string Id { get; set; } = "";
        /// <summary>已執行的工具名稱。</summary>
        public string Name { get; set; } = "";
        /// <summary>要回送模型的工具執行結果。</summary>
        public object Response { get; set; } = new { };
    }
}

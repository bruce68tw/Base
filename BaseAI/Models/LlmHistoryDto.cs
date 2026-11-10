namespace BaseAI.Models
{
    /// <summary>即時對話歷史中的一則文字訊息。</summary>
    public sealed class LlmHistoryDto
    {
        /// <summary>訊息角色，例如 user 或 assistant。</summary>
        public string Role { get; set; } = string.Empty;
        /// <summary>該則歷史訊息的文字。</summary>
        public string Text { get; set; } = string.Empty;
    }
}
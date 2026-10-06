namespace BaseAI.Models
{
    public sealed class LlmHistoryDto
    {
        public string Role { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
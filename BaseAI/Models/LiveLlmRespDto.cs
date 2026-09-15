using BaseAI.Enums;

namespace BaseAI.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class LiveLlmRespDto
    {
        public LiveLlmRespTypeEnum Type { get; set; }

        public byte[]? Audio { get; set; }

        public string? Text { get; set; }

        public string? MimeType { get; set; }

        public IReadOnlyCollection<LiveLlmToolCallDto>? ToolCalls { get; set; }
    }
}

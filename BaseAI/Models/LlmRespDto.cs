using BaseAI.Enums;

namespace BaseAI.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class LlmRespDto
    {
        public LlmRespTypeEnum Type { get; set; }

        public byte[]? Audio { get; set; }

        public string? Text { get; set; }

        public string? MimeType { get; set; }

        public IReadOnlyCollection<LiveLlmToolCallDto>? ToolCalls { get; set; }

        /// <summary>Usage 事件的本回合 token 總數（provider 回報的累計值）。</summary>
        public int TotalTokens { get; set; }
    }
}

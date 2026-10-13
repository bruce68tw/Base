namespace BaseAI.Models
{
    public sealed record LlmTurnResultDto(
        string UserText,
        string LlmText,
        int TotalTokens,
        byte[] InputAudio,
        bool IsBreak,
        //new speech to text
        bool NeedSTT)
    {
        public List<LlmHistoryDto> History { get; init; } = [];
    }
}
namespace BaseAI.Models
{
    public sealed record LlmTurnResultDto(
        string UserText,
        string LlmText,
        int TotalTokens,
        byte[] InputAudio,
        bool IsBreak,
        bool NeedSpeechToText);
}
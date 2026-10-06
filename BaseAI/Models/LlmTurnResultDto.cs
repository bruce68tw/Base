namespace BaseAI.Models
{
    public sealed record LlmTurnResultDto(
        string UserText,
        string AssistantText,
        int TotalTokens,
        byte[] InputAudio,
        bool WasInterrupted,
        bool RequiresSpeechToText);
}
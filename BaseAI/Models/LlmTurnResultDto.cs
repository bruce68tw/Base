namespace BaseAI.Models
{
    /// <summary>提供應用處理或保存的即時對話回合快照。</summary>
    /// <param name="UserText">本回合的使用者文字。</param>
    /// <param name="LlmText">本回合累積的模型回覆文字。</param>
    /// <param name="TotalTokens">供應商回報的本回合權杖總數。</param>
    /// <param name="InputAudio">本回合保存的使用者輸入音訊。</param>
    /// <param name="IsBreak">是否為中斷的回合。</param>
    /// <param name="NeedSTT">交付回合前是否需要以輸入音訊補做語音辨識。</param>
    public sealed record LlmTurnResultDto(
        string UserText,
        string LlmText,
        int TotalTokens,
        byte[] InputAudio,
        bool IsBreak,
        //new speech to text
        bool NeedSTT)
    {
        /// <summary>與回合一起交付的對話歷史快照。</summary>
        public List<LlmHistoryDto> History { get; init; } = [];
    }
}
namespace BaseAI.Models
{
    /// <summary>
    /// 呼叫端為單一 Live LLM session 註冊的非同步處理器。
    /// </summary>
    public sealed class LiveLlmSessionHandlers
    {
        /// <summary>回合整理完成時呼叫，供應用層執行回合完成規則。</summary>
        public required Func<LlmTurnResultDto, CancellationToken, Task> OnTurnFinishAsync { get; init; }

        /// <summary>收到工具呼叫時執行；未註冊時由橋接層回覆不支援工具。</summary>
        public Func<LlmToolCallDto, CancellationToken, Task<object>>? ToolCallAsync { get; init; }
    }
}
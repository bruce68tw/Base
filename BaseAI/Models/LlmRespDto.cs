using BaseAI.Enums;

namespace BaseAI.Models
{
    /// <summary>上游即時模型事件的共用資料，依 Type 填入相應欄位。</summary>
    public class LlmRespDto
    {
        /// <summary>事件類型，用於判斷應讀取的資料欄位。</summary>
        public LlmRespTypeEnum Type { get; set; }

        /// <summary>音訊事件的位元組資料。</summary>
        public byte[]? Audio { get; set; }

        /// <summary>文字、轉錄或錯誤事件的內容。</summary>
        public string? Text { get; set; }

        /// <summary>音訊事件的 MIME 類型。</summary>
        public string? MimeType { get; set; }

        /// <summary>工具呼叫事件附帶的呼叫清單。</summary>
        public List<LlmToolCallDto>? ToolCalls { get; set; }

        /// <summary>Usage 事件的本回合 token 總數（provider 回報的累計值）。</summary>
        public int TotalTokens { get; set; }
    }
}

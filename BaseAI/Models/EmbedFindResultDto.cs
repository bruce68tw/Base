namespace BaseAI.Models
{
    /// <summary>向量檢索結果的資料模型。 ()</summary>
    public class EmbedFindResultDto
    {
        /// <summary>檢索命中的項目 ID。</summary>
        public string Id { get; set; } = "";
        /// <summary>命中項目的文字。</summary>
        public string Text { get; set; } = "";
        /// <summary>todo:待確認 此類別未在現行 BaseAI 查詢流程使用，分數的範圍與排序方向待確認。</summary>
        public float Score { get; set; }
        /// <summary>命中項目的附加欄位，可為 null。</summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}

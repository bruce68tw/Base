namespace BaseAI.Models
{
    /// <summary>向量資料庫讀回的單筆文件資料。 ()</summary>
    public class EmbedRowDto
    {
        /// <summary>向量資料庫中的項目 ID。</summary>
        public string Id { get; set; } = "";
        /// <summary>與向量項目一併讀回的文件文字。</summary>
        public string Text { get; set; } = "";
        /// <summary>與該文件項目一併讀回的嵌入向量。</summary>
        public float[] Embedding { get; set; } = [];
        /// <summary>與文件一併保存的附加欄位，可為 null。</summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}

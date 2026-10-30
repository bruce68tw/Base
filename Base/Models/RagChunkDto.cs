namespace Base.Models
{
    /// <summary>
    /// RAG 問答片段資料，短屬性名配合來源 JSON。
    /// </summary>
    public class RagChunkDto
    {
        /// <summary>
        /// 問題文字。
        /// </summary>
        public string Q { get; set; } = "";
        /// <summary>
        /// 回答文字。
        /// </summary>
        public string A { get; set; } = "";
        /// <summary>
        /// 是否為切分片段的標記，預設 false。
        /// </summary>
        public bool C { get; set; } = false;
    }

}

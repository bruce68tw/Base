namespace BaseAI.Models
{
    /// <summary>
    /// 後端 locale resource for base class
    /// 調用 _Json.CopyModel(), 必須宣告為類別屬性 !!
    /// </summary>
    public class EmbedDocuDto
    {
        public string Id { get; set; } = "";
        public string Text { get; set; } = "";
        public float[] Embedding { get; set; } = [];
        public Dictionary<string, object>? Metadata { get; set; }
    }
}

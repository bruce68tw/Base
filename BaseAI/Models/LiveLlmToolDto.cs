namespace BaseAI.Models
{
    public sealed class LiveLlmToolDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public object Parameters { get; set; } = new { };
    }

    public sealed class LiveLlmToolCallDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string ArgumentsJson { get; set; } = "{}";
    }

    public sealed class LiveLlmToolRespDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public object Response { get; set; } = new { };
    }
}

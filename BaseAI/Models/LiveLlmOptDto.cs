namespace BaseAI.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class LiveLlmOptDto
    {
        public string EndPoint { get; set; } = "";
        public string Model { get; set; } = "";
        public string VoiceName { get; set; } = "";
        public string SystemPrompt { get; set; } = "";
        public string Language { get; set; } = "zh-TW";
        public bool AudioResponse { get; set; } = true;
        public IReadOnlyCollection<LiveLlmToolDto> Tools { get; set; } = Array.Empty<LiveLlmToolDto>();
    }
}

namespace BaseWeb.Models
{
    public class XiCheckTextDto : XiBaseDto
    {
        public bool IsCheck { get; set; } = false;
        public string Label { get; set; } = "";
    }
}
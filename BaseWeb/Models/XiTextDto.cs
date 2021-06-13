namespace BaseWeb.Models
{
    public class XiTextDto : XiBaseDto
    {
        public XiTextDto()
        {
            MaxLen = 0;
            Type = "text";
        }

        public int MaxLen { get; set; }
        public string Type { get; set; }
    }
}
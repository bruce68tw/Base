namespace BaseWeb.Models
{
    public class XiTextDto : XiBaseDto
    {
        public XiTextDto()
        {
            MaxLen = 0;
            IsPwd = false;
        }

        public int MaxLen { get; set; }
        public bool IsPwd { get; set; }
    }
}
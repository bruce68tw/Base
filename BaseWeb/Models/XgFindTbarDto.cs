namespace BaseWeb.Models
{
    public class XgFindTbarDto
    {
        public XgFindTbarDto()
        {
            IsHori = true;
            HasReset = false;
            HasFind2 = false;
        }

        public bool IsHori { get; set; }
        public bool HasReset { get; set; }
        public bool HasFind2 { get; set; }
    }
}
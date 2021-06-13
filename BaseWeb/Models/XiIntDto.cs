namespace BaseWeb.Models
{
    public class XiIntDto : XiBaseDto
    {
        public XiIntDto()
        {
            Min = 0;
            Max = 0;
        }

        public int Min { get; set; }
        public int Max { get; set; }
    }
}
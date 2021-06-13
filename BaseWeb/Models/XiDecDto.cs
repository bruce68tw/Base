namespace BaseWeb.Models
{
    public class XiDecDto : XiBaseDto
    {
        public XiDecDto()
        {
            Min = 0;
            Max = 0;
        }

        public decimal Min { get; set; }
        public decimal Max { get; set; }
    }
}
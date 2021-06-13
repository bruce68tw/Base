namespace BaseWeb.Models
{
    public class XiTaDto : XiBaseDto
    {
        public XiTaDto()
        {
            MaxLen = 0;
            RowsCount = 3;
        }

        public int MaxLen { get; set; }
        public int RowsCount { get; set; }
    }
}
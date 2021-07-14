namespace BaseWeb.Models
{
    public class XiTextareaDto : XiBaseDto
    {
        public XiTextareaDto()
        {
            MaxLen = 0;
            RowsCount = 3;
        }

        public int MaxLen { get; set; }
        public int RowsCount { get; set; }
    }
}
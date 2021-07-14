namespace BaseWeb.Models
{
    public class XiHtmlDto : XiBaseDto
    {
        public XiHtmlDto()
        {
            MaxLen = 0;
            //RowsCount = 10;
        }

        public int MaxLen { get; set; }
        //public int RowsCount { get; set; }
    }
}
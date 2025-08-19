namespace BaseWeb.Models
{
    public class XiDtDto : XiBaseDto
    {
        /*
        public XiDtDto()
        {
            MinuteStep = 10;
        }
        */

        public int MinuteStep { get; set; } = 10;

        public string FnOnChange { get; set; } = "";

    }
}
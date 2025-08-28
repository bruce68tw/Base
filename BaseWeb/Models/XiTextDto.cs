using Base.Enums;

namespace BaseWeb.Models
{
    public class XiTextDto : XiBaseDto
    {
        /*
        public XiTextDto()
        {
            MaxLen = 0;
            IsPwd = false;
        }
        */

        public int MaxLen { get; set; } = 0;
        public bool IsPwd { get; set; } = false;

        /// <summary>
        /// 對應 InputPatternEstr, 如為中文, 則會
        /// </summary>
        public string Pattern { get; set; } = InputPatternEstr.EngNumExt;

    }
}
using Base.Enums;

namespace BaseWeb.Models
{
    public class XiTextareaDto : XiBaseDto
    {
        /*
        public XiTextareaDto()
        {
            MaxLen = 0;
            RowsCount = 3;
        }
        */

        public int MaxLen { get; set; } = 0;
        public int RowsCount { get; set; } = 3;

        /// <summary>
        /// 對應 InputPatternEstr
        /// </summary>
        public string Pattern { get; set; } = InputPatternEstr.EngNumExt;
    }
}
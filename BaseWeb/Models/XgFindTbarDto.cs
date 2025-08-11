namespace BaseWeb.Models
{
    public class XgFindTbarDto
    {
        /*
        public XgFindTbarDto()
        {
            IsHori = true;
            HasReset = false;
            HasFind2 = false;
        }
        */

        //span mode, default false, 表示工具列會包在col裡面, 如果true, 則此工具列會和緊鄰左側欄位
        public bool SpanMode { get; set; } = false;
        public bool IsHori { get; set; } = true;
        public bool HasReset { get; set; } = false;
        public bool HasFind2 { get; set; } = false;

        /// <summary>
        /// 傳回bool
        /// </summary>
        //public string FnWhenFind { get; set; } = "";

    }
}
namespace BaseWeb.Models
{
    /// <summary>設定查詢工具列的版面與可選操作。</summary>
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
        /// <summary>是否讓工具列緊接在左側欄位後方。</summary>
        public bool SpanMode { get; set; } = false;
        /// <summary>是否採水平配置。</summary>
        public bool IsHori { get; set; } = true;
        /// <summary>是否顯示重設查詢條件的按鈕。</summary>
        public bool HasReset { get; set; } = false;
        /// <summary>是否顯示第二個查詢按鈕。</summary>
        public bool HasFind2 { get; set; } = false;

        /// <summary>
        /// 傳回bool
        /// </summary>
        //public string FnWhenFind { get; set; } = "";

    }
}
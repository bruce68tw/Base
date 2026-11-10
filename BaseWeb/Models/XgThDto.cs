namespace BaseWeb.Models
{
    /// <summary>設定表格欄標題、提示與顯示樣式。</summary>
    public class XgThDto
    {
        /*
        public XgThDto()
        {
            Title = "";
            Tip = "";
            Required = false;
            ClsExt = "";
            HideRwd = false;
            MinWidth = 0;
        }
        */

        /// <summary>表頭顯示的標題。</summary>
        public string Title { get; set; } = "";
        /// <summary>表頭的說明提示文字。</summary>
        public string Tip { get; set; } = "";
        /// <summary>是否顯示必填標記。</summary>
        public bool Required { get; set; } = false;

        //ExtClass -> ClsExt
        /// <summary>附加到表頭的 CSS 類別。</summary>
        public string ClsExt { get; set; } = "";

        //min width, 0 for not set
        /// <summary>表頭最小寬度，為零時不設定。</summary>
        public int MinWidth { get; set; } = 0;

        //hide for RWD phone
        /// <summary>是否在窄版響應式畫面隱藏表頭。</summary>
        public bool HideRwd { get; set; } = false;
    }
}
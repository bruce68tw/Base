namespace BaseAI.Models
{
    /// <summary>由前端傳入的第二階段欄位語意對應請求。</summary>
    public class VoiceProgArgVo
    {
        /// <summary>現有語音逐字稿，用來補足條件的上下文。</summary>
        public string VoiceText { get; set; } = "";

        /// <summary>畫面輸入欄位的 ID，與 UiNames、OptNames 逐筆對齊。</summary>
        public List<string> UiFids { get; set; } = [];

        /// <summary>畫面欄位的中文顯示名稱清單。</summary>
        public List<string> UiNames { get; set; } = [];

        /// <summary>UiNames 的下拉式欄位的中文顯示名稱清單。</summary>
        public List<List<string>> OptNames { get; set; } = [];

        /// <summary>待對應的條件清單，保留原始順序及每筆條件的多個值。</summary>
        /// <remarks>本階段僅使用 VoiceName、VoiceValues；VoiceName 可為空白，不使用 Name、Values。</remarks>
        //public List<VoiceProgArgDto> VoiceArgs { get; set; } = [];

        /// <summary>畫面欄位的中文顯示名稱清單，不是欄位 ID 或說明。</summary>
        //public List<string> UiNames { get; set; } = [];
    }

}

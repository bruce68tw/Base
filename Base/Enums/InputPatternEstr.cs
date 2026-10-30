namespace Base.Enums
{
    /// <summary>
    /// 輸入欄位的 pattern 設定；None 由前端欄位產生器視為不設 pattern。
    /// </summary>
    public class InputPatternEstr
    {
        /// <summary>
        /// 不限制此 pattern，並非只允許中文。
        /// </summary>
        public const string None = "none";
        /// <summary>
        /// 一個以上的 ASCII 英文字母或數字。
        /// </summary>
        public const string EngNum = "[A-Za-z0-9]+";
        /// <summary>
        /// 英數、括號、連字號、斜線及空白；文字與多行欄位的預設 pattern。
        /// </summary>
        public const string EngNumExt = "[A-Za-z0-9\\(\\)\\-\\/ ]+";
    }
}

namespace Base.Enums
{
    /// <summary>
    /// _Str.RandomStr 使用的隨機字元集合，並非密碼學安全亂數設定。
    /// </summary>
    public enum RandomTypeEnum
    {
        /// <summary>
        /// 數字 0 到 9，列舉預設值。
        /// </summary>
        Num,
        /// <summary>
        /// 大寫英文字母，不含 I、O。
        /// </summary>
        UpEng,
        /// <summary>
        /// 大寫英文字母（不含 I、O）與數字。
        /// </summary>
        UpEngNum,
        /// <summary>
        /// 大小寫英文字母與數字；大寫部分不含 I、O，不含符號。
        /// </summary>
        All,
    }
}

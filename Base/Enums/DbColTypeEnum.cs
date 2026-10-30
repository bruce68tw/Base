namespace Base.Enums
{
    /// <summary>
    /// Db 讀取資料列時使用的欄位轉換分類；此型別是整數常數類別。
    /// </summary>
    public class DbColTypeEnum
    {
        /// <summary>
        /// 日期時間，輸出完整日期時間字串。
        /// </summary>
        public const int Datetime = 1;
        /// <summary>
        /// 日期，輸出日期字串。
        /// </summary>
        public const int Date = 2;
        /// <summary>
        /// 布林值，輸出 1 或 0。
        /// </summary>
        public const int Bit = 3;
        /// <summary>
        /// 整數，轉為 Int32。
        /// </summary>
        public const int Int = 4;
        /// <summary>
        /// 數值，轉為 Decimal。
        /// </summary>
        public const int Decimal = 5;
        /// <summary>
        /// 其他欄位，依一般字串處理。
        /// </summary>
        public const int Other = 9;
    }
}

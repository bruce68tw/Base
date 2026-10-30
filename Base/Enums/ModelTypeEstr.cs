namespace Base.Enums
{
    /// <summary>
    /// Excel 匯入時使用的模型欄位轉換分類。
    /// </summary>
    public static class ModelTypeEstr
    {
        /// <summary>
        /// 日期時間轉換。
        /// </summary>
        public const string Datetime = "Datetime";
        /// <summary>
        /// 整數轉換。
        /// </summary>
        public const string Int = "Int";
        /// <summary>
        /// 字串或其他欄位的預設分類。
        /// </summary>
        public const string String = "String";
    }
}

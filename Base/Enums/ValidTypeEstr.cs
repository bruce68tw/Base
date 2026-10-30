namespace Base.Enums
{
    /// <summary>
    /// 前端欄位驗證的類型標記。
    /// </summary>
    public class ValidTypeEstr
    {
        /// <summary>
        /// 不指定此驗證類型，值為空字串。
        /// </summary>
        public const string None = "";
        /// <summary>
        /// 電子郵件類型標記。
        /// </summary>
        public const string Email = "Email";
        /// <summary>
        /// 電話類型標記。
        /// </summary>
        public const string Phone = "Phone";
        /// <summary>
        /// HTML 類型標記。
        /// </summary>
        public const string Html = "Html";
    }
}

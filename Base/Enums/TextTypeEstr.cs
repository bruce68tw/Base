namespace Base.Enums
{
    /// <summary>
    /// 文字輸入與前端驗證使用的類型字串；不保證後端套用同名驗證。
    /// </summary>
    public class TextTypeEstr
    {
        /// <summary>
        /// 一般文字。
        /// </summary>
        public const string Text = "text";
        /// <summary>
        /// 密碼。
        /// </summary>
        public const string Password = "password";
        /// <summary>
        /// 電子郵件。
        /// </summary>
        public const string Email = "email";
        /// <summary>
        /// 網址。
        /// </summary>
        public const string Url = "url";
        /// <summary>
        /// 信用卡驗證的類型標記。
        /// </summary>
        public const string CreditCard = "creditcard";
    }
}

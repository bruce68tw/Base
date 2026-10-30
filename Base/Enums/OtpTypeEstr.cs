namespace Base.Enums
{
    /// <summary>
    /// 一次性驗證碼的驗證管道代碼；各管道是否可用依服務設定。
    /// </summary>
    public class OtpTypeEstr
    {
        /// <summary>
        /// 電子郵件驗證碼。
        /// </summary>
        public const string Email = "E";
        /// <summary>
        /// 電子郵件連結的類型標記。
        /// </summary>
        public const string EmailLink = "EL";
        /// <summary>
        /// 簡訊驗證碼。
        /// </summary>
        public const string Sms = "S";
        /// <summary>
        /// 驗證器應用程式的類型標記。
        /// </summary>
        public const string Auth = "A";
    }
}

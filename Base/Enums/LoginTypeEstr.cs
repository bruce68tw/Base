namespace Base.Enums
{
    /// <summary>
    /// 登入方式代碼，供組態與登入流程判斷使用。
    /// </summary>
    public class LoginTypeEstr
    {
        /// <summary>
        /// 不要求登入，_Fun.IsNeedLogin 以此值判斷。
        /// </summary>
        public const string None = "0";
        /// <summary>
        /// 帳號密碼登入，為 ConfigDto 的預設設定。
        /// </summary>
        public const string Pwd = "Pwd";
        /// <summary>
        /// 透過 AD／LDAP 驗證登入。
        /// </summary>
        public const string Ad = "Ad";
    }
}

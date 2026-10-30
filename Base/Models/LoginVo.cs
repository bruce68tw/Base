namespace Base.Models
{
    /// <summary>
    /// 登入表單的輸入及錯誤提示資料。
    /// </summary>
    public class LoginVo
    {
        /// <summary>
        /// 登入帳號。
        /// </summary>
        public string Account { get; set; } = "";
        /// <summary>
        /// 登入密碼。
        /// </summary>
        public string Pwd { get; set; } = "";
        //public string Locale { get; set; }
        /// <summary>
        /// 登入後返回的來源網址設定。
        /// </summary>
        public string FromUrl { get; set; } = "";
        /// <summary>
        /// 帳號欄位的提示或錯誤。
        /// </summary>
        public string AccountMsg { get; set; } = "";
        /// <summary>
        /// 密碼欄位的提示或錯誤。
        /// </summary>
        public string PwdMsg { get; set; } = "";
        /// <summary>
        /// 整體登入錯誤訊息。
        /// </summary>
        public string ErrorMsg { get; set; } = "";
    }
}

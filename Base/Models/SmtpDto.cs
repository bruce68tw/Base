
namespace Base.Models
{
    /// <summary>
    /// 寄送郵件使用的 SMTP 連線與寄件者設定。
    /// </summary>
    public class SmtpDto
    {
        /// <summary>
        /// 寄件者電子郵件地址。
        /// </summary>
        public string FromEmail { get; set; } = "";
        /// <summary>
        /// 收件者看到的寄件者名稱。
        /// </summary>
        public string FromName { get; set; } = "";
        /// <summary>
        /// SMTP 主機名稱。
        /// </summary>
        public string Host { get; set; } = "";
        /// <summary>
        /// SMTP 連接埠號。
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// SMTP 驗證帳號。
        /// </summary>
        public string Id { get; set; } = "";
        /// <summary>
        /// SMTP 驗證密碼。
        /// </summary>
        public string Pwd { get; set; } = "";
        /// <summary>
        /// 是否啟用 SMTP SSL，初值為 true。
        /// </summary>
        public bool Ssl = true;
    }
}

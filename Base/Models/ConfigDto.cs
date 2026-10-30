using Base.Enums;
using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// 應用載入後指派給 _Fun.Config 的共用設定；此模型不會自行讀取組態檔。
    /// </summary>
    public class ConfigDto
    {
        /// <summary>
        /// 是否在初始化時解密敏感組態欄位，預設 false。
        /// </summary>
        public bool Encode { get; set; } = false;
        /// <summary>
        /// 上傳檔案加密的設定旗標；本身不執行加密。
        /// </summary>
        public bool EncodeUpload { get; set; } = false;
        /// <summary>
        /// 登入方式代碼，預設為帳號密碼登入。
        /// </summary>
        public string LoginType { get; set; } = LoginTypeEstr.Pwd;
        /// <summary>
        /// LDAP 伺服器、網域與選用連接埠，以逗號分隔；預設連接埠為 389。
        /// </summary>
        public string AdServer { get; set; } = "";
        /// <summary>
        /// 預設資料庫連線字串；Encode 啟用時由初始化流程解密。
        /// </summary>
        public string Db { get; set; } = "";
        /// <summary>
        /// 系統顯示名稱。
        /// </summary>
        public string SystemName { get; set; } = "MIS System";
        /// <summary>
        /// 系統英文名稱，可供應用作為識別名稱。
        /// </summary>
        public string SystemEngName { get; set; } = "MIS System";
        /// <summary>
        /// 預設語系代碼，初值為 zh-TW。
        /// </summary>
        public string Locale { get; set; } = "zh-TW";
        /// <summary>
        /// 產生識別碼時使用的主機代碼。
        /// </summary>
        public string ServerId { get; set; } = "";
        /// <summary>
        /// 慢 SQL 記錄門檻，單位毫秒；大於 0 時啟用，預設 1000。
        /// </summary>
        public int SlowSql { get; set; } = 1000;
        /// <summary>
        /// 是否輸出除錯記錄。
        /// </summary>
        public bool LogDebug { get; set; }
        /// <summary>
        /// 是否輸出 SQL 記錄。
        /// </summary>
        public bool LogSql { get; set; }
        /// <summary>
        /// 系統錯誤通知的管理者信箱。
        /// </summary>
        public string RootEmail { get; set; } = "";
        /// <summary>
        /// 測試收件者信箱；非空時 _Email 會改用此收件者。
        /// </summary>
        public string TesterEmail { get; set; } = "";
        /// <summary>
        /// 上傳檔案大小上限設定，單位 MB，預設 5；是否套用依上傳端。
        /// </summary>
        public int UploadFileMax { get; set; } = 5;

        //cache time(second)
        //public int CacheSecond = "";
        /// <summary>
        /// SSL 設定旗標；與 SMTP 字串內的 Ssl 欄位分開。
        /// </summary>
        public bool SSL { get; set; }
        /// <summary>
        /// 逗號分隔 Host、Port、Ssl、Id、Pwd、FromEmail、FromName，共七欄。
        /// </summary>
        public string Smtp { get; set; } = "";
        /// <summary>
        /// 郵件圖片的 Id 與路徑交錯清單，以逗號分隔。
        /// </summary>
        public string EmailImagePairs { get; set; } = "";
        /// <summary>
        /// HTML 圖片根網址，Init 會補上尾端斜線。
        /// </summary>
        public string HtmlImageUrl { get; set; } = "";
        /// <summary>
        /// Redis 連線設定字串；不會自行註冊快取服務。
        /// </summary>
        public string Redis { get; set; } = "";
        /// <summary>
        /// 允許的 CORS 來源設定，以逗號分隔。
        /// </summary>
        public string AllowOrigins { get; set; } = "";
        /// <summary>
        /// Google OAuth 用戶端識別碼。
        /// </summary>
        public string GoogleClientId { get; set; } = "";
        /// <summary>
        /// Google OAuth 用戶端密鑰。
        /// </summary>
        public string GoogleClientSecret { get; set; } = "";
        /// <summary>
        /// Google OAuth 重新導向網址。
        /// </summary>
        public string GoogleRedirect { get; set; } = "";
        /// <summary>
        /// Facebook OAuth 用戶端識別碼。
        /// </summary>
        public string FbClientId { get; set; } = "";
        /// <summary>
        /// Facebook OAuth 用戶端密鑰。
        /// </summary>
        public string FbClientSecret { get; set; } = "";
        /// <summary>
        /// Facebook OAuth 重新導向網址。
        /// </summary>
        public string FbRedirect { get; set; } = "";
        /// <summary>
        /// 電子郵件驗證碼有效分鐘數，預設 3。
        /// </summary>
        public int OtpEmailMin { get; set; } = 3;
        /// <summary>
        /// 電子郵件驗證連結有效分鐘數設定，預設 5。
        /// </summary>
        public int OtpEmailLinkMin { get; set; } = 5;
        /// <summary>
        /// 簡訊驗證碼有效分鐘數，預設 3。
        /// </summary>
        public int OtpSmsMin { get; set; } = 3;
        /// <summary>
        /// 驗證器時間設定，單位秒，預設 60。
        /// </summary>
        public int OtpAuthSec { get; set; } = 60;
        /// <summary>
        /// 驗證器使用的 Base32 密鑰。
        /// </summary>
        public string OtpAuthKey { get; set; } = "";
        /// <summary>
        /// 簡訊驗證碼訊息範本。
        /// </summary>
        public string OtpSmsTpl { get; set; } = "";
        /// <summary>
        /// 簡訊服務帳號。
        /// </summary>
        public string SmsAccount { get; set; } = "";
        /// <summary>
        /// 簡訊服務密碼。
        /// </summary>
        public string SmsPwd { get; set; } = "";
        /// <summary>
        /// 簡訊服務提供的來源門號。
        /// </summary>
        public string SmsFromPhone { get; set; } = "";
        /// <summary>
        /// 允許呼叫服務的來源 IP 清單，初值為空清單。
        /// </summary>
        public List<string> ClientIps { get; set; } = [];
    }
}

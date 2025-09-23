using Base.Enums;

namespace Base.Models
{
    /// <summary>
    /// get from appSettings.json Config section
    /// </summary>
    public class ConfigDto
    {
        //是否加密組態檔重要欄位, ex: Db,Smtp,Redis
        public bool Encode { get; set; } = false;

        //refer LoginTypeEstr
        public string LoginType { get; set; } = LoginTypeEstr.None;

        public string AdServer { get; set; } = "";

        //db connect string
        public string Db { get; set; } = "";

        //system name
        public string SystemName { get; set; } = "MIS System";

        //可做為 Issuer
        public string SystemEngName { get; set; } = "MIS System";

        //default locale code
        public string Locale { get; set; } = "zh-TW";

        //server Id for new key
        public string ServerId { get; set; } = "";

        //log error for slow sql(mini secode)
        public int SlowSql { get; set; } = 1000;

        //log debug
        public bool LogDebug { get; set; }

        //log sql
        public bool LogSql { get; set; }

        //root email address for send error
        public string RootEmail { get; set; } = "";

        //tester email address
        public string TesterEmail { get; set; } = "";

        //upload file max size(MB)
        public int UploadFileMax { get; set; } = 5;

        //cache time(second)
        //public int CacheSecond = "";

        //SSL or not
        public bool SSL { get; set; }

        //smtp, format: 0(Host),1(Port),2(Ssl),3(Id),4(Pwd),5(FromEmail),6(FromName) 
        public string Smtp { get; set; } = "";

        //email image path list: Id,Path.., ex: _TopImage, c:/xx/xx.png
        public string EmailImagePairs { get; set; } = "";

        /// <summary>
        /// html image root url for sublime, ex: http://xxx.xx/image, auto add right slash
        /// </summary>
        public string HtmlImageUrl { get; set; } = "";

        /// <summary>
        /// redis server for session, ex: "127.0.0.1:6379,ssl=true,password=xxx,defaultDatabase=x", 
        /// empty for memory cache
        /// </summary>
        public string Redis { get; set; } = "";

        /// <summary>
        /// for CORS, 逗號分隔
        /// empty for memory cache
        /// </summary>
        public string AllowOrigins { get; set; } = "";

        /// <summary>
        /// google client Id for oAuth2
        /// </summary>
        public string GoogleClientId { get; set; } = "";

        /// <summary>
        /// google client Secret for oAuth2
        /// </summary>
        public string GoogleClientSecret { get; set; } = "";

        /// <summary>
        /// google oAuth2 redirect url
        /// </summary>
        public string GoogleRedirect { get; set; } = "";

        /// <summary>
        /// facebook client Id for oAuth2
        /// </summary>
        public string FbClientId { get; set; } = "";

        /// <summary>
        /// facebook client Secret for oAuth2
        /// </summary>
        public string FbClientSecret { get; set; } = "";

        /// <summary>
        /// facebook oAuth2 redirect url
        /// </summary>
        public string FbRedirect { get; set; } = "";

        /// <summary>
        /// otp email 有效時間(分)
        /// </summary>
        public int OtpEmailMin { get; set; } = 3;

        /// <summary>
        /// otp email link 有效時間(分)
        /// </summary>
        public int OtpEmailLinkMin { get; set; } = 5;

        /// <summary>
        /// otp sms 有效時間(分)
        /// </summary>
        public int OtpSmsMin { get; set; } = 3;

        /// <summary>
        /// otp Authenticator 有效時間(秒)
        /// </summary>
        public int OtpAuthSec { get; set; } = 60;

        /// <summary>
        /// otp Authenticator secret key, base32
        /// </summary>
        public string OtpAuthKey { get; set; } = "";

        /// <summary>
        /// 簡訊帳號
        /// </summary>
        public string SmsAccount { get; set; } = "";
        /// <summary>
        /// 簡訊密碼
        /// </summary>
        public string SmsPwd { get; set; } = "";
        /// <summary>
        /// 簡訊來源門號, 由廠商提供
        /// </summary>
        public string SmsFromPhone { get; set; } = "";
    }
}

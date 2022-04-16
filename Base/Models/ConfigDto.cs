namespace Base.Models
{
    /// <summary>
    /// get from appSettings.json Config section
    /// </summary>
    public class ConfigDto
    {
        //constructor
        //must use { get; set; } for binding at Startup.cs !!
        public ConfigDto()
        {
            SystemName = "MIS System";
            Locale = "zh-TW";
            ServerId = "A";
            SlowSql = 1000;
            //LogDebug = false;
            //LogSql = false;
            //RootEmail = "";
            //TesterEmail = "";
            UploadFileMax = 5;
            //CacheSecond = 3600;
            //SSL = false;
            //Smtp = "";
            //HtmlImageUrl = "";
            //Redis = "";
            HtmlImageUrl = "";
        }

        //db connect string
        public string Db { get; set; }

        //system name
        public string SystemName { get; set; }

        //default locale code
        public string Locale { get; set; }

        //server Id for new key
        public string ServerId { get; set; }

        //log error for slow sql(mini secode)
        public int SlowSql { get; set; }

        //log debug
        public bool LogDebug { get; set; }

        //log sql
        public bool LogSql { get; set; }

        //root email address for send error
        public string RootEmail { get; set; }

        //tester email address
        public string TesterEmail { get; set; }

        //upload file max size(MB)
        public int UploadFileMax { get; set; }

        //cache time(second)
        //public int CacheSecond = "";

        //SSL or not
        public bool SSL { get; set; }

        //smtp, format: 0(Host),1(Port),2(Ssl),3(Id),4(Pwd),5(FromEmail),6(FromName) 
        public string Smtp { get; set; }

        //email image path list: Id,Path.., ex: _TopImage, c:/xx/xx.png
        public string EmailImagePairs { get; set; }

        /// <summary>
        /// html image root url for sublime, ex: http://xxx.xx/image, auto add right slash
        /// </summary>
        public string HtmlImageUrl { get; set; }

        /// <summary>
        /// redis server for session, ex: "127.0.0.1:6379,ssl=true,password=xxx,defaultDatabase=x", 
        /// empty for memory cache
        /// </summary>
        public string Redis { get; set; }

        /// <summary>
        /// google client Id for oAuth2
        /// </summary>
        public string GoogleClientId { get; set; }

        /// <summary>
        /// google client Secret for oAuth2
        /// </summary>
        public string GoogleClientSecret { get; set; }

        /// <summary>
        /// google oAuth2 redirect url
        /// </summary>
        public string GoogleRedirect { get; set; }

        /// <summary>
        /// facebook client Id for oAuth2
        /// </summary>
        public string FbClientId { get; set; }

        /// <summary>
        /// facebook client Secret for oAuth2
        /// </summary>
        public string FbClientSecret { get; set; }

        /// <summary>
        /// facebook oAuth2 redirect url
        /// </summary>
        public string FbRedirect { get; set; }

    }
}

namespace Base.Models
{
    /// <summary>
    /// get from appSettings.json Config section
    /// </summary>
    public class ConfigDto
    {
        //constructor
        public ConfigDto()
        {
            SystemName = "管理系統";
            DefaultLocale = "zh-TW";
            //FrontDtFormat = "yyyy/M/d HH:mm:ss";
            ServerId = "A";
            SlowSql = 1000;
            LogInfo = false;
            LogSql = false;
            RootMail = "";
            TesterMail = "";
            UploadFileMax = 5;
            CacheSecond = 3600;
            SSL = false;
            Smtp = "";
        }

        //db connect string
        public string Db { get; set; }

        //system name
        public string SystemName { get; set; }

        //default locale code
        public string DefaultLocale { get; set; }

        //front datetime format for: (1)front UI, (2)db read datetime column
        //public string FrontDtFormat { get; set; }
        
        //server Id for new key
        public string ServerId { get; set; }

        //log error for slow sql(mini secode)
        public int SlowSql { get; set; }

        //log info
        public bool LogInfo { get; set; }

        //log sql
        public bool LogSql { get; set; }

        //root email address for send error
        public string RootMail { get; set; }

        //tester email address
        public string TesterMail { get; set; }

        //upload file max size(MB)
        public int UploadFileMax { get; set; }

        //cache time(second)
        public int CacheSecond { get; set; }

        //SSL or not
        public bool SSL { get; set; }

        //smtp, format: 0(Host),1(Port),2(Ssl),3(Id),4(Pwd),5(FromEmail),6(FromName) 
        public string Smtp { get; set; }

    }
}

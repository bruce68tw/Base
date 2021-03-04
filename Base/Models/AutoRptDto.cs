namespace Base.Models
{
    /// <summary>
    /// 自動報表
    /// </summary>
    public class AutoRptDto
    {
        //report name
        public string RptName { get; set; }

        //db property in config file
        public string Db { get; set; }

        //sql statement
        public string Sql { get; set; }

        //excel template file
        //public string Template { get; set; }

        //excel start row no(base 0) for data
        public int SrcRowNo { get; set; }

        //email list, seperate with ',' or ';'
        public string Emails { get; set; }

    }
}

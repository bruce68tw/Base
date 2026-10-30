namespace Base.Models
{
    /// <summary>
    /// 以頁碼描述的簡易分頁輸入；不代表現行 CRUD 已提供對應查詢入口。
    /// </summary>
    public class PageIn
    {
        /// <summary>
        /// 初始化為第 1 頁、每頁 10 筆，篩選筆數為 -1。
        /// </summary>
        public PageIn()
        {
            page = 1;
            length = 10;
            filterRows = -1;
            findJson = "";
        }

        //Draw counter, jQuery Datatables need for control async ajax sequence
        //public int draw { get; set; }

        //start row position, base 0
        //public int start { get; set; }
        /// <summary>
        /// 頁碼，從 1 起算。
        /// </summary>
        public int page { get; set; }
        /// <summary>
        /// 每頁筆數，建構時預設 10。
        /// </summary>
        public int length { get; set; }
        /// <summary>
        /// 篩選後筆數，-1 表示需重新計算。
        /// </summary>
        public int filterRows { get; set; }
        /// <summary>
        /// 欄位查詢條件的 JSON 字串，建構時為空字串。
        /// </summary>
        public string findJson { get; set; }

        //search word
        //public DtSearchDto search { get; set; }

        //public List<DtOrderDto> order { get; set; }

    }

}

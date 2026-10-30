namespace Base.Models
{
    /// <summary>
    /// 簡化列表查詢輸入，供 CrudReadSvc 內部條件處理使用。
    /// </summary>
    public class EasyDtDto
    {
        /*
        public EasyDtDto()
        {
            start = 0;
            length = 10;
            recordsFiltered = -1;
            findJson = "";
            sort = "";
        }
        */

        //Draw counter, jQuery Datatables need for control async ajax sequence
        //public int draw { get; set; }
        /// <summary>
        /// 起始資料列位置，從 0 起算。
        /// </summary>
        public int start { get; set; } = 0;
        /// <summary>
        /// 本頁筆數，預設 10。
        /// </summary>
        public int length { get; set; } = 10;
        /// <summary>
        /// 篩選後總筆數，預設 -1 表示重新計算。
        /// </summary>
        public int recordsFiltered { get; set; } = -1;
        /// <summary>
        /// 欄位查詢條件的 JSON 字串。
        /// </summary>
        public string findJson { get; set; } = "";
        /// <summary>
        /// A 或 D 加上排序欄位名稱。
        /// </summary>
        public string sort { get; set; } = "";
        /// <summary>
        /// 快速搜尋文字，對應 DtDto.search.value。
        /// </summary>
        public string search2 { get; set; } = "";

        //search word
        //public DtSearchDto search { get; set; }

    }

}

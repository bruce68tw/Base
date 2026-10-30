using Base.Enums;
using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// DataTables 列表查詢輸入；屬性名稱保留前端格式。
    /// </summary>
    public class DtDto
    {
        /// <summary>
        /// 前端請求序號。
        /// </summary>
        public int draw { get; set; } = 0;
        /// <summary>
        /// 起始資料列位置，從 0 起算。
        /// </summary>
        public int start { get; set; } = 0;
        /// <summary>
        /// 本頁筆數，初值為 0，需由呼叫端指定。
        /// </summary>
        public int length { get; set; } = 0;
        /// <summary>
        /// 篩選後總筆數；-1 表示重新計算，初值為 0。
        /// </summary>
        public int recordsFiltered { get; set; } = 0;
        /// <summary>
        /// 欄位查詢條件的 JSON 字串。
        /// </summary>
        public string findJson { get; set; } = "";
        /// <summary>
        /// 優先排序設定：A 或 D 加上欄位名稱，直接參與 SQL 組合。
        /// </summary>
        public string sort { get; set; } = "";
        /// <summary>
        /// 全域快速搜尋條件，可為 null。
        /// </summary>
        public DtSearchDto? search { get; set; } = null;
        /// <summary>
        /// 前端排序條件，sort 為空時採用首項，可為 null。
        /// </summary>
        public List<DtOrderDto>? order { get; set; } = null;

    }
    /// <summary>
    /// DataTables 的單項排序設定。
    /// </summary>
    public class DtOrderDto
    {
        /// <summary>
        /// 前端欄位索引，從 0 起算。
        /// </summary>
        public int column { get; set; } = 0;
        /// <summary>
        /// 排序欄位名稱；現行查詢轉換使用此值。
        /// </summary>
        public string fid { get; set; } = "";
        /// <summary>
        /// 排序方向，預設升冪。
        /// </summary>
        public OrderTypeEnum dir { get; set; } = OrderTypeEnum.Asc;
    }
    /// <summary>
    /// DataTables 的快速搜尋設定。
    /// </summary>
    public class DtSearchDto
    {
        /// <summary>
        /// 快速搜尋文字。
        /// </summary>
        public string value { get; set; } = "";
        /// <summary>
        /// 正規表示式旗標；現行查詢未依此旗標處理。
        /// </summary>
        public bool regex { get; set; } = false;
    }
}

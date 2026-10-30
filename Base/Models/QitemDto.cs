using Base.Enums;

namespace Base.Models
{
    /// <summary>
    /// 單一查詢輸入與資料欄位的對應及條件設定。
    /// </summary>
    public class QitemDto
    {
        /// <summary>
        /// 前端查詢欄位識別名稱。
        /// </summary>
        public string Fid = "";
        /// <summary>
        /// SQL 欄位或運算片段，可含表別名；空白時由 TableAs 與 Fid 組成。
        /// </summary>
        public string Col = "";
        /// <summary>
        /// 查詢運算代碼，預設等值比對。
        /// </summary>
        public string Op = QitemOpEstr.Equal;
        /// <summary>
        /// 日期等特殊查詢型別，預設 None 依 Op 處理。
        /// </summary>
        public QitemTypeEnum Type = QitemTypeEnum.None;        
        /// <summary>
        /// 額外查詢資訊；Date2 時指定另一個日期資料欄位。
        /// </summary>
        public string Other = "";
    }
}

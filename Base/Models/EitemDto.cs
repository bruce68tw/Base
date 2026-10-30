using Base.Enums;

namespace Base.Models
{
    /// <summary>
    /// 單一 CRUD 編輯欄位的讀寫及驗證設定。
    /// </summary>
    public class EitemDto
    {
        /// <summary>
        /// 輸入 JSON 的欄位識別名稱。
        /// </summary>
        public string Fid = "";
        /// <summary>
        /// 資料表欄位對應名稱；空白時沿用 Fid。
        /// </summary>
        public string Col = "";
        /// <summary>
        /// 驗證種類，預設 CheckTypeEstr.None。
        /// </summary>
        public string CheckType = CheckTypeEstr.None;
        /// <summary>
        /// 搭配 CheckType 的驗證參數；數值比較依目前檢查分支實作。
        /// </summary>
        public string CheckData = "";
        /// <summary>
        /// 新增欄位的預設值。
        /// </summary>
        public object? Value;
        /// <summary>
        /// 是否必填，預設 false。
        /// </summary>
        public bool Required = false;
        /// <summary>
        /// 唯讀旗標，為 true 時略過新增與修改。
        /// </summary>
        public bool Read = false;
        /// <summary>
        /// 是否允許新增時寫入，預設 true。
        /// </summary>
        public bool Create = true;
        /// <summary>
        /// 是否允許修改時寫入，預設 true。
        /// </summary>
        public bool Update = true;
        /// <summary>
        /// 是否以 HTML 內容處理；儲存時使用 HtmlSanitizer 清理。
        /// </summary>
        public bool IsHtml = false;
    }
}

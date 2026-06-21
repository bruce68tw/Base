using Base.Enums;

namespace Base.Models
{
    /// <summary>
    /// 用來設定一個CRUD編輯欄位
    /// </summary>
    public class EitemDto
    {
        /// <summary>
        /// 前端UI的欄位Id
        /// </summary>
        public string Fid = "";

        /// <summary>
        /// 要存取的資料表欄位名稱，包含 table alias，如果空白表示同 Fid 欄位
        /// </summary>
        public string Col = "";

        /// <summary>
        /// 資料驗證方式，參考 CheckTypeEstr.cs
        /// </summary>
        public string CheckType = CheckTypeEstr.None;

        //check data for CheckType, ex:
        //
        /// <summary>
        /// 資料驗證的資料內容，配合 CheckType 欄位，例如: CheckType="Min", CheckData="1"，則表示最小值必須大於等於1
        /// </summary>
        public string CheckData = "";

        /// <summary>
        /// 新增一筆資料時這個Fid欄位的預設值
        /// </summary>
        public object? Value;

        /// <summary>
        /// required or not
        /// </summary>
        public bool Required = false;

        /// <summary>
        /// if true, Create/Update will skip
        /// </summary>
        public bool Read = false;

        /// <summary>
        /// Fid欄位是否允許新增
        /// </summary>
        public bool Create = true;

        /// <summary>
        /// Fid欄位是否允許修改
        /// </summary>
        public bool Update = true;

        /// <summary>
        /// Fid欄位是否為HTML, 如果是則系統會自動編碼/解碼
        /// </summary>
        public bool IsHtml = false;
    }
}

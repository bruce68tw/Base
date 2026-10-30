namespace Base.Enums
{
    /// <summary>
    /// CRUD 編輯欄位的驗證種類；實際檢查依 CrudEditSvc 實作。
    /// </summary>
    public class CheckTypeEstr
    {
        /// <summary>
        /// 不套用此類型驗證，為 EitemDto 的預設設定。
        /// </summary>
        public const string None = "0";
        /// <summary>
        /// 電子郵件格式檢查。
        /// </summary>
        public const string Email = "Email";
        /// <summary>
        /// 網址類型標記；目前 CRUD 後端分支略過檢查。
        /// </summary>
        public const string Url = "Url";
        /// <summary>
        /// 目前實作在值大於 CheckData 時回報錯誤，與一般最小值語意不同。
        /// </summary>
        public const string Min = "Min";
        /// <summary>
        /// 目前實作在值小於 CheckData 時回報錯誤，與一般最大值語意不同。
        /// </summary>
        public const string Max = "Max";
        /// <summary>
        /// 數值須落在 CheckData 指定的逗號分隔上下限內，包含端點。
        /// </summary>
        public const string Range = "Range";
    }
}

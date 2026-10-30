namespace Base.Enums
{
    /// <summary>
    /// CRUD 功能及權限字串的位置索引；AuthRow 到 Other 的數值對應字串位置。
    /// </summary>
    public enum CrudEnum
    {
        /// <summary>
        /// 位置 0，表示是否啟用資料列權限，並非一般 CRUD 操作。
        /// </summary>
        AuthRow,
        /// <summary>
        /// 新增。
        /// </summary>
        Create,
        /// <summary>
        /// 列表查詢。
        /// </summary>
        Read,
        /// <summary>
        /// 修改。
        /// </summary>
        Update,
        /// <summary>
        /// 刪除。
        /// </summary>
        Delete,
        /// <summary>
        /// 列印。
        /// </summary>
        Print,
        /// <summary>
        /// 匯出。
        /// </summary>
        Export,
        /// <summary>
        /// 檢視單筆資料。
        /// </summary>
        View,
        /// <summary>
        /// 其他功能。
        /// </summary>
        Other,
        /// <summary>
        /// 未指定子功能的標記值 99；CheckAuth 以作業是否存在判斷。
        /// </summary>
        Empty = 99
    }
}

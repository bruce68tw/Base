namespace Base.Enums
{
    /// <summary>
    /// 資料庫種類，供初始化及 SQL 方言選擇使用；列舉存在不代表全部功能均已支援。
    /// </summary>
    public enum DbTypeEnum
    {
        /// <summary>
        /// 未指定資料庫種類，列舉預設值；Init 沒有對應設定分支。
        /// </summary>
        None,
        /// <summary>
        /// Microsoft SQL Server。
        /// </summary>
        MSSql,
        /// <summary>
        /// MySQL；Init 的刪除 SQL 設定仍有待實作項目。
        /// </summary>
        MySql,
        /// <summary>
        /// Oracle；Init 的刪除 SQL 設定仍有待實作項目。
        /// </summary>
        Oracle
    }
}

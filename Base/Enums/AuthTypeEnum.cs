namespace Base.Enums
{
    /// <summary>
    /// 功能授權的檢查層級，由 _Fun.AuthType 設定。
    /// </summary>
    public enum AuthTypeEnum
    {
        /// <summary>
        /// 不啟用功能授權層級；不代表所有入口會自動略過檢查。
        /// </summary>
        None,
        /// <summary>
        /// 控制器層級，判斷是否可進入作業。
        /// </summary>
        Ctrl,
        /// <summary>
        /// 功能層級，分別判斷新增、查詢等操作權限。
        /// </summary>
        Action,
        /// <summary>
        /// 資料列層級，搭配個人或部門等資料範圍限制。
        /// </summary>
        Row,
    }
}

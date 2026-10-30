namespace Base.Enums
{
    /// <summary>
    /// 查詢欄位的型別提示；日期種類可改用專用條件處理。
    /// </summary>
    public enum QitemTypeEnum
    {
        /// <summary>
        /// 不使用專用日期分支，依 Op 組合條件；為 QitemDto 預設值。
        /// </summary>
        None,
        /// <summary>
        /// 單一日期欄位的起迄查詢，第二個輸入鍵為 Fid 加上 2。
        /// </summary>
        Date,
        /// <summary>
        /// 兩個日期欄位的區間交集查詢，Other 指定另一個資料欄位。
        /// </summary>
        Date2,
        /// <summary>
        /// 日期時間的型別標記；目前 CrudReadSvc 沒有專用分支。
        /// </summary>
        DT,
    }
}

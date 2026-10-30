namespace Base.Enums
{
    /// <summary>
    /// 表示單一 CRUD 功能的資料存取範圍，由 _Auth.GetAuthRange 解析權限字串取得。
    /// </summary>
    /// <remarks>
    /// 權限字串以單一數字表示範圍，數值變更會影響解析；未啟用資料列權限時，非零權限轉為 All。
    /// </remarks>
    public enum AuthRangeEnum
    {
        /// <summary>
        /// 無功能權限（預設值 0）；不代表略過權限檢查。
        /// </summary>
        None,       //0
        /// <summary>
        /// 個人資料範圍。
        /// </summary>
        User,       //1
        /// <summary>
        /// 部門資料範圍；實際限制依呼叫端實作。
        /// </summary>
        Dept,       //2
        /// <summary>
        /// 全部資料範圍，仍受其他查詢條件與授權檢查限制。
        /// </summary>
        All = 9,
    }
}

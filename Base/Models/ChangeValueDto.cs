namespace Base.Models
{
    /// <summary>
    /// 編輯資料的異動前後值。
    /// </summary>
    public class ChangeValueDto
    {
        /// <summary>
        /// 異動項目的識別鍵。
        /// </summary>
        public string Key { get; set; } = "";
        /// <summary>
        /// 異動前的字串值。
        /// </summary>
        public string OldValue { get; set; } = "";
        /// <summary>
        /// 異動後的字串值。
        /// </summary>
        public string NewValue { get; set; } = "";
    }
}


namespace Base.Models
{
    /// <summary>
    /// 檔案匯入的統計及錯誤結果。
    /// </summary>
    public class ResultImportDto
    {
        /// <summary>
        /// 此次匯入記錄的識別碼。
        /// </summary>
        public string LogRowId = "";
        /// <summary>
        /// 成功匯入筆數。
        /// </summary>
        public int OkCount;
        /// <summary>
        /// 失敗筆數。
        /// </summary>
        public int FailCount;
        /// <summary>
        /// 總處理筆數。
        /// </summary>
        public int TotalCount;
        /// <summary>
        /// 整體匯入錯誤訊息。
        /// </summary>
        public string _ErrorMsg = "";

    }
}

using BaseAI.Models;

namespace BaseAI.Interfaces
{
    /// <summary>
    /// 嵌入式向量資料庫服務的抽象基底類別。
    /// 定義向量文件的新增、更新、讀取、刪除、相似度查詢與清除操作；
    /// 提供共用的 collection/table 名稱解析與 URL 組合工具。
    /// </summary>
    public abstract class AbEmbedDbSvc(string embedDbStr, string embedTable, int embedDim,
        double maxDistance, int queryRows) : IAsyncDisposable
    {
        /// <summary>供衍生類別執行資料庫 HTTP 請求的用戶端。</summary>
        protected HttpClient _httpClient = new();

        /// <summary>向量資料庫服務的連線位址或 API 根路徑。</summary>
        protected string _embedDbStr = embedDbStr;

        /// <summary>未指定 collection/table 時採用的預設名稱。</summary>
        protected string _embedTable = embedTable;

        /// <summary>向量預期維度。</summary>
        protected int _embedDim = embedDim;

        /// <summary>相似度查詢的預設最大距離。</summary>
        protected double _maxDistance = maxDistance;

        /// <summary>相似度查詢的預設候選筆數。</summary>
        protected int _queryRows = queryRows;

        protected string _nowTable = string.Empty;
        protected string _nowTableId = string.Empty;

        protected abstract Task<bool> SetNowTableA(string table);

        /*
        protected string TableUrl() =>
            $"{_embedDbStr.TrimEnd('/')}/{_nowTable}";
        */

        protected string ActionUrl(string action) =>
            $"{_embedDbStr}/{_nowTableId}/{action}";

        /// <summary>
        /// 新增一筆向量及其文件識別資訊。
        /// </summary>
        /// <param name="id">資料庫中的向量項目唯一識別碼。</param>
        /// <param name="vector">要保存的 embedding 向量。</param>
        /// <param name="fileId">此向量所屬來源文件的識別碼，供後續依文件刪除使用。</param>
        /// <param name="table">目標 collection/table；空字串表示由實作採用預設名稱。</param>
        /// <returns>新增成功時回傳 <see langword="true"/>，否則回傳 <see langword="false"/>。</returns>
        public abstract Task<bool> CreateA(string table, string id, float[] vector, string fileId);

        /// <summary>
        /// 更新既有向量項目的 embedding 與來源文件識別資訊。
        /// </summary>
        /// <param name="id">要更新的向量項目唯一識別碼。</param>
        /// <param name="vector">取代原內容的新 embedding 向量。</param>
        /// <param name="fileId">此向量所屬來源文件的識別碼。</param>
        /// <param name="table">目標 collection/table；空字串表示由實作採用預設名稱。</param>
        /// <returns>更新成功時回傳 <see langword="true"/>，否則回傳 <see langword="false"/>。</returns>
        public abstract Task<bool> UpdateA(string table, string id, float[] vector, string fileId);

        /// <summary>
        /// 依向量項目識別碼讀取文件及其 embedding／metadata。
        /// </summary>
        /// <param name="id">要讀取的向量項目唯一識別碼。</param>
        /// <param name="table">來源 collection/table；空字串表示由實作採用預設名稱。</param>
        /// <returns>找到時回傳文件資料；找不到時回傳 <see langword="null"/>。</returns>
        public abstract Task<EmbedDocuDto?> GetA(string table, string id);

        /// <summary>
        /// 依向量項目識別碼刪除單筆資料。
        /// </summary>
        /// <param name="id">要刪除的向量項目唯一識別碼。</param>
        /// <param name="table">目標 collection/table；空字串表示由實作採用預設名稱。</param>
        /// <returns>代表非同步刪除操作的工作。</returns>
        public abstract Task<bool> DeleteA(string table, string id);

        /// <summary>
        /// 刪除指定來源文件所建立的向量資料。
        /// </summary>
        /// <param name="fileId">來源文件的識別碼。</param>
        /// <param name="table">目標 collection/table；空字串表示由實作採用預設名稱。</param>
        /// <returns>刪除成功時回傳 <see langword="true"/>，否則回傳 <see langword="false"/>。</returns>
        public abstract Task<bool> DeleteByFileIdA(string table, string fileId);

        /// <summary>
        /// 以輸入向量執行相似度查詢，回傳符合查詢條件的向量項目識別資訊。
        /// </summary>
        /// <param name="vector">查詢用 embedding 向量，其維度應符合資料庫 collection 的設定。</param>
        /// <param name="maxDistance">本次查詢採用的最大距離；為 0 時是否改用服務預設值由實作決定。</param>
        /// <param name="queryRows">本次查詢要求的候選筆數；為 0 時是否改用服務預設值由實作決定。</param>
        /// <param name="table">查詢目標 collection/table；空字串表示由實作採用預設名稱。</param>
        /// <returns>查詢結果字串；結果內容與序列化格式由衍生類別定義。</returns>
        public abstract Task<List<string>> VectorToIdsA(string table, float[] vector, double maxDistance = 0, int queryRows = 0);

        /// <summary>
        /// 以輸入向量執行相似度查詢，將符合距離條件的文件內容合併為文字。
        /// </summary>
        /// <param name="vector">查詢用 embedding 向量，其維度應符合資料庫 collection 的設定。</param>
        /// <param name="maxDistance">本次查詢採用的最大距離；為 0 時是否改用服務預設值由實作決定。</param>
        /// <param name="queryRows">本次查詢要求的候選筆數；為 0 時是否改用服務預設值由實作決定。</param>
        /// <param name="table">查詢目標 collection/table；空字串表示由實作採用預設名稱。</param>
        /// <returns>符合條件的文件文字；沒有符合結果時回傳空字串。</returns>
        //public abstract Task<string> VectorToTextA(float[] vector, decimal maxDistance = 0, int queryRows = 0);

        /// <summary>
        /// 清除由此服務實作所管理的向量資料；清除範圍由衍生類別定義。
        /// </summary>
        /// <returns>代表非同步清除操作的工作。</returns>
        //public abstract Task ClearA();

        /// <summary>
        /// 釋放此服務使用的 HTTP 用戶端及其相關資源。
        /// </summary>
        /// <returns>代表非同步釋放操作的工作。</returns>
        public async ValueTask DisposeAsync()
        {
            _httpClient.Dispose();
            _httpClient = null!;

            await ValueTask.CompletedTask;
            GC.SuppressFinalize(this);
        }

    }
}


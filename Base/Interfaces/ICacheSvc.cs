using System.Threading.Tasks;

namespace Base.Interfaces
{
    /// <summary>
    /// 以使用者識別與鍵存取字串快取；期限與重設範圍依實作。
    /// </summary>
    public interface ICacheSvc
    {
        /// <summary>
        /// 讀取字串；查無值或 Redis 服務未就緒時可回傳 null。
        /// </summary>
        /// <param name="userId">用來識別使用者的 ID</param>
        /// <param name="key">用來定位資料的鍵值</param>
        string? GetStr(string userId, string key);
        /// <summary>
        /// 寫入字串並回傳操作結果；過期策略依實作。
        /// </summary>
        /// <param name="userId">用來識別使用者的 ID</param>
        /// <param name="key">用來定位資料的鍵值</param>
        /// <param name="value">要使用的輸入值</param>
        bool SetStr(string userId, string key, string value);
        /// <summary>
        /// 刪除指定使用者的鍵；布林結果依實作判定。
        /// </summary>
        /// <param name="userId">用來識別使用者的 ID</param>
        /// <param name="key">用來定位資料的鍵值</param>
        bool DeleteKey(string userId, string key);
        /// <summary>
        /// 非同步重設快取；現有實作清除整個記憶體快取或目前 Redis 資料庫，非單一使用者。
        /// </summary>
        Task ResetDbA();
    }
}

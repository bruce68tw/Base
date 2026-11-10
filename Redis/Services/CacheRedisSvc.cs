using Base.Interfaces;
using Base.Services;
using StackExchange.Redis;

namespace Redis.Services
{
    /// <summary>以 StackExchange.Redis 實作共用快取介面，使用同步操作讀寫字串。 (use StackExchange)</summary>
    public class CacheRedisSvc : ICacheSvc
    {
        private readonly IDatabase _db = null!;     //current redis database
        private readonly bool _status;      //redis connection status
        //private string _preKey = "";
        private IConnectionMultiplexer _redis = null!;

        //constructor
        /// <summary>由既有連線取得 Redis 資料庫，初始化失敗時記錄錯誤。</summary>
        /// <param name="redis">呼叫端提供的 Redis 連線多工器</param>
        public CacheRedisSvc(IConnectionMultiplexer redis)
        {
            try
            {
                _redis = redis;
                _db = redis.GetDatabase();
                //_preKey = _Fun.UserId() + "_";
                _status = true;
            }
            catch(Exception ex)
            {
                _status = false;
                _Log.Error("CacheRedisService.cs failed: " + ex.Message);    //discard result !!
            }
        }

        private string GetKey(string userId, string key)
        {
            return userId + "_" + key;
        }

        /// <summary>依使用者與鍵讀取字串。未就緒或查無資料時回傳 null。</summary>
        /// <param name="key">要讀取的快取鍵，服務會加上使用者識別值作為前綴。</param>
        /// <param name="userId">用來識別使用者的 ID</param>
        /// <returns>快取的字串，查無資料時回傳 null。</returns>
        public string? GetStr(string userId, string key)
        {
            if (!_status) return null;
            var value = _db.StringGet(GetKey(userId, key));
            return (value.IsNull)
                ? null : value.ToString();
        }

        /// <summary>依使用者與鍵寫入字串，回傳 Redis 操作結果。</summary>
        /// <param name="userId">用來識別使用者的 ID</param>
        /// <param name="key">用來定位資料的鍵值</param>
        /// <param name="value">用於初始化欄位或選項的值</param>
        public bool SetStr(string userId, string key, string value)
        {
            if (!_status) return false;
            return _db.StringSet(GetKey(userId, key), value);
        }

        /// <summary>刪除使用者的指定快取鍵，未就緒時回傳 false。</summary>
        /// <param name="userId">用來識別使用者的 ID</param>
        /// <param name="key">用來定位資料的鍵值</param>
        public bool DeleteKey(string userId, string key)
        {
            if (!_status) return false;
            return _db.KeyDelete(GetKey(userId, key));
        }

        /*
        */
        /// <summary>連線就緒時，對第一個 Redis 端點呼叫資料庫清除操作。</summary>
        public async Task ResetDbA()
        {
            if (!_status) return;
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            await server.FlushDatabaseAsync();
        }

    }//class
}

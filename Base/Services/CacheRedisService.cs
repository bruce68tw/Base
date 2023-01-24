using StackExchange.Redis;
using System;

namespace Base.Services
{
    /// <summary>
    /// Redis cache server(use StackExchange)
    /// 配合 MemoryCache 使用同步取值
    /// </summary>
    public class CacheRedisService : ICacheService
    {
        private readonly IDatabase _db;     //current redis database
        private readonly bool _status;      //redis connection status
        private string _preKey;

        //constructor
        public CacheRedisService(IConnectionMultiplexer redis)
        {
            try
            {
                _db = redis.GetDatabase();
                _preKey = _Fun.UserId() + "_";
                _status = true;
            }
            catch(Exception ex)
            {
                _status = false;
                _ = _Log.ErrorA("CacheRedisService.cs failed: " + ex.Message);    //discard result !!
            }
        }

        private string GetKey(string key)
        {
            return _preKey + key;
        }

        /// <summary>
        /// get string value
        /// </summary>
        /// <param name="key"></param>
        /// <returns>null if not found</returns>
        public string GetStr(string key)
        {
            if (!_status) return null;

            var value = _db.StringGet(GetKey(key));
            return (value.IsNull)
                ? null
                : value.ToString();
        }

        public bool SetStr(string key, string value)
        {
            if (!_status) return false;

            return _db.StringSet(GetKey(key), value);
        }

        public bool DeleteKey(string key)
        {
            if (!_status) return false;

            return _db.KeyDelete(GetKey(key));
        }

        /*
        /// <summary>
        /// delete all keys in current database index
        /// </summary>
        public Task FlushDbA()
        {
            if (!IsOk())
                return;

            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            await server.FlushDatabaseAsync();
        }
        */

    }//class
}

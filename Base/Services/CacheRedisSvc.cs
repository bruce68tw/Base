using Base.Interfaces;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// Redis cache server(use StackExchange)
    /// 配合 MemoryCache 使用同步取值
    /// </summary>
    public class CacheRedisSvc : ICacheS
    {
        private readonly IDatabase _db = null!;     //current redis database
        private readonly bool _status;      //redis connection status
        //private string _preKey = "";
        private IConnectionMultiplexer _redis = null!;

        //constructor
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

        /// <summary>
        /// get string value
        /// </summary>
        /// <param name="key"></param>
        /// <returns>null if not found</returns>
        public string? GetStr(string userId, string key)
        {
            if (!_status) return null;
            var value = _db.StringGet(GetKey(userId, key));
            return (value.IsNull)
                ? null : value.ToString();
        }

        public bool SetStr(string userId, string key, string value)
        {
            if (!_status) return false;
            return _db.StringSet(GetKey(userId, key), value);
        }

        public bool DeleteKey(string userId, string key)
        {
            if (!_status) return false;
            return _db.KeyDelete(GetKey(userId, key));
        }

        /*
        */
        /// <summary>
        /// delete all keys in current database index
        /// </summary>
        public async Task ResetDbA()
        {
            if (!_status) return;
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            await server.FlushDatabaseAsync();
        }

    }//class
}

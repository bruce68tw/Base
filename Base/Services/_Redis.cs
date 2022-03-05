using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// use StackExchange Redis currently
    /// </summary>
    public static class _Redis
    {
        private static readonly ConnectionMultiplexer _redis;
        private static readonly IDatabase _db;   //current database

        //constructor
        static _Redis()
        {
            try
            {
                _redis = ConnectionMultiplexer.Connect(_Fun.Config.Redis);
                _db = _redis.GetDatabase();
            }
            catch(Exception ex)
            {
                _redis = null;
                _ = _Log.ErrorAsync("_Redis.cs failed: " + ex.Message);    //discard result !!
            }
        }

        //is redis status ok or not
        private static bool IsOk()
        {
            return (_redis != null);
        }

        /// <summary>
        /// get string value
        /// </summary>
        /// <param name="key"></param>
        /// <returns>null if not found</returns>
        public static async Task<string> GetStrAsync(string key)
        {
            if (!IsOk())
                return null;

            var value = await _db.StringGetAsync(key);
            return (value.IsNull)
                ? null
                : value.ToString();
        }

        public static async Task<bool> SetStrAsync(string key, string value)
        {
            if (!IsOk())
                return false;

            return await _db.StringSetAsync(key, value);
        }

        public static async Task<bool> DeleteKeyAsync(string key)
        {
            if (!IsOk())
                return false;

            return await _db.KeyDeleteAsync(key);
        }

        /// <summary>
        /// delete all keys in current database index
        /// </summary>
        public static async Task FlushDbAsync()
        {
            if (!IsOk())
                return;

            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            await server.FlushDatabaseAsync();
        }

    }//class
}

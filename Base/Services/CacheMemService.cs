using Microsoft.Extensions.Caching.Memory;
using System;

namespace Base.Services
{
    //memory cache server
    public class CacheMemService : ICacheService
    {
        private readonly IMemoryCache _cache;
        //private string _preKey;

        //constructor
        public CacheMemService(IMemoryCache cache)
        {
            _cache = cache;
            //_preKey = _Fun.UserId() + "_";
        }

        private string GetKey(string key)
        {
            return $"{_Fun.UserId()}_{key}";
        }

        public string GetStr(string key)
        {
            _cache.TryGetValue(GetKey(key), out string result);
            return result;
        }

        public bool SetStr(string key, string value)
        {
            _cache.Set(GetKey(key), value, new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(_Fun.TimeOut)));
            return true;
        }

        public bool DeleteKey(string key)
        {
            _cache.Remove(GetKey(key));
            return true;
        }

        /*
        /// <summary>
        /// delete all keys in current database index
        /// </summary>
        public async Task FlushDbA()
        {
            if (!IsOk())
                return;

            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            await server.FlushDatabaseAsync();
        }
        */

    } //class
}

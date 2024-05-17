using Base.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Base.Services
{
    //memory cache server
    public class CacheMemSvc : ICacheSvc
    {
        private readonly IMemoryCache _cache;
        //private string _preKey;

        //constructor
        public CacheMemSvc(IMemoryCache cache)
        {
            _cache = cache;
            //_preKey = _Fun.UserId() + "_";
        }

        private string GetKey(string userId, string key)
        {
            return userId + "_" + key;
        }

        public string? GetStr(string userId, string key)
        {
            _cache.TryGetValue(GetKey(userId, key), out string? result);
            return result ?? null;
        }

        public bool SetStr(string userId, string key, string value)
        {
            _cache.Set(GetKey(userId, key), value, new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(_Fun.TimeOut)));
            return true;
        }

        public bool DeleteKey(string userId, string key)
        {
            _cache.Remove(GetKey(userId, key));
            return true;
        }

        /*
        */
        /// <summary>
        /// delete all keys in current database index
        /// </summary>
        public async Task ResetDbA()
        {
            (_cache as MemoryCache)!.Compact(1.0);
        }

    } //class
}

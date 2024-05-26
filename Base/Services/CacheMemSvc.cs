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
#pragma warning disable CS1998 // Async 方法缺乏 'await' 運算子，將同步執行
        public async Task ResetDbA()
#pragma warning restore CS1998 // Async 方法缺乏 'await' 運算子，將同步執行
        {
            (_cache as MemoryCache)!.Compact(1.0);
        }

    } //class
}

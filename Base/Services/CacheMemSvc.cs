using Base.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// 以 userId_鍵 存取 IMemoryCache 的實作。
    /// </summary>
    public class CacheMemSvc : ICacheSvc
    {
        private readonly IMemoryCache _cache;
        //private string _preKey;

        /// <summary>
        /// 使用傳入的記憶體快取，保留其生命週期管理。
        /// </summary>
        public CacheMemSvc(IMemoryCache cache)
        {
            _cache = cache;
            //_preKey = _Fun.UserId() + "_";
        }

        private string GetKey(string userId, string key)
        {
            return userId + "_" + key;
        }

        /// <summary>
        /// 讀取字串，找不到時回傳 null。
        /// </summary>
        public string? GetStr(string userId, string key)
        {
            _cache.TryGetValue(GetKey(userId, key), out string? result);
            return result ?? null;
        }

        /// <summary>
        /// 寫入字串，滑動期限為 _Fun.TimeOut 分鐘；完成時回傳 true。
        /// </summary>
        public bool SetStr(string userId, string key, string value)
        {
            _cache.Set(GetKey(userId, key), value, new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(_Fun.TimeOut)));
            return true;
        }

        /// <summary>
        /// 移除指定鍵；即使鍵不存在也回傳 true。
        /// </summary>
        public bool DeleteKey(string userId, string key)
        {
            _cache.Remove(GetKey(userId, key));
            return true;
        }

        /*
        */
#pragma warning disable CS1998 // Async 方法缺乏 'await' 運算子，將同步執行
        /// <summary>
        /// 清除整個 MemoryCache；實作同步執行，要求底層可轉成 MemoryCache。
        /// </summary>
        public async Task ResetDbA()
#pragma warning restore CS1998 // Async 方法缺乏 'await' 運算子，將同步執行
        {
            (_cache as MemoryCache)!.Compact(1.0);
        }

    } //class
}

using Base.Interfaces;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// 透過 _Fun.DiBox 取得 ICacheSvc 的靜態快取入口；須先註冊服務。
    /// </summary>
    public class _Cache
    {

        private static ICacheSvc GetService()
        {
            return (ICacheSvc)_Fun.DiBox!.GetService(typeof(ICacheSvc))!;
        }

        /// <summary>
        /// 讀取使用者與鍵對應的字串，無值時可回傳 null。
        /// </summary>
        public static string? GetStr(string userId, string key)
        {
            return GetService().GetStr(userId, key);
        }

        /// <summary>
        /// 寫入字串，回傳底層快取實作的操作結果。
        /// </summary>
        public static bool SetStr(string userId, string key, string value)
        {
            return GetService().SetStr(userId, key, value);
        }

        /// <summary>
        /// 將快取 JSON 反序列化為模型；無值時回傳 default。
        /// </summary>
        public static T? GetModel<T>(string userId, string key)
        {
            var str = GetService().GetStr(userId, key);
            return _Str.IsEmpty(str)
                ? default : _Str.ToModel<T>(str!);
        }

        /// <summary>
        /// 將模型序列化為 JSON 並寫入快取。
        /// </summary>
        public static bool SetModel<T>(string userId, string key, T model)
        {
            return GetService().SetStr(userId, key, _Model.ToJsonStr(model));
        }

        /// <summary>
        /// 刪除指定使用者的鍵，結果依底層實作。
        /// </summary>
        public static bool DeleteKey(string userId, string key)
        {
            return GetService().DeleteKey(userId, key);
        }

        /// <summary>
        /// 非同步重設快取；現有實作影響整個快取或 Redis 資料庫，非單一使用者。
        /// </summary>
        public static async Task ResetDbA()
        {
            await GetService().ResetDbA();
        }
    }
}

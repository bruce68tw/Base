using Base.Services;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BaseApi.Extensions
{
    /// <summary>
    /// 以 JSON 字串讀寫 ASP.NET Core Session 中的型別化資料。鍵不存在時讀取結果為型別的預設值。
    /// </summary>
    public static class SessionExtension
    {
        /// <summary>
        /// 以 JSON 字串將指定值存入目前 Session。
        /// </summary>
        /// <param name="session">要寫入的目前 Session。</param>
        /// <param name="key">儲存值所使用的 Session 鍵。</param>
        /// <param name="value">要序列化成 JSON 的值。</param>
        public static void Set<T>(this ISession session, string key, T value)
        {
            //var http = _Http.GetHttp();
            //session.SetString(key, JsonSerializer.Serialize(value));
            session.SetString(key, _Model.ToJsonStr(value));
        }

        /// <summary>
        /// 讀取指定 Session 鍵並反序列化為 T。鍵不存在時回傳 T 的預設值。
        /// </summary>
        /// <param name="session">要讀取的目前 Session。</param>
        /// <param name="key">要查詢的 Session 鍵。</param>
        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            //return value == null ? default : JsonSerializer.Deserialize<T>(value);
            return (value == null) 
                ? default : JsonConvert.DeserializeObject<T>(value);
        }

    } //class
}

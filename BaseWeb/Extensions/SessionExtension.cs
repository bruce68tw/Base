using Base.Services;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
//using System.Text.Json;

namespace BaseWeb.Extensions
{
    public static class SessionExtension
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            //session.SetString(key, JsonSerializer.Serialize(value));
            session.SetString(key, _Model.ToJsonStr(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            //return value == null ? default : JsonSerializer.Deserialize<T>(value);
            return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        }

    } //class
}

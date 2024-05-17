using Base.Interfaces;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// 將 Cache class 包裝成靜態類別, 存取 ICacheService 介面
    /// </summary>
    public class _Cache
    {

        private static ICacheSvc GetService()
        {
            return (ICacheSvc)_Fun.DiBox!.GetService(typeof(ICacheSvc))!;
        }

        public static string? GetStr(string userId, string key)
        {
            return GetService().GetStr(userId, key);
        }

        public static bool SetStr(string userId, string key, string value)
        {
            return GetService().SetStr(userId, key, value);
        }

        public static T? GetModel<T>(string userId, string key)
        {
            var str = GetService().GetStr(userId, key);
            return (_Str.IsEmpty(str)) 
                ? default : _Model.JsonStrToModel<T>(str!);
        }

        public static bool SetModel<T>(string userId, string key, T model)
        {
            return GetService().SetStr(userId, key, _Model.ToJsonStr(model));
        }

        public static bool DeleteKey(string userId, string key)
        {
            return GetService().DeleteKey(userId, key);
        }

        public static async Task ResetDbA()
        {
            await GetService().ResetDbA();
        }
    }
}

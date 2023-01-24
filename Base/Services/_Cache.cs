using DocumentFormat.OpenXml.Spreadsheet;

namespace Base.Services
{
    /// <summary>
    /// 將 Cache class 包裝成靜態類別, 存取 ICacheService 介面
    /// </summary>
    public class _Cache
    {

        private static ICacheService GetService()
        {
            return (ICacheService)_Fun.DiBox.GetService(typeof(ICacheService));
        }

        public static string GetStr(string key)
        {
            return GetService().GetStr(key);
        }

        public static bool SetStr(string key, string value)
        {
            return GetService().SetStr(key, value);
        }

        public static T GetModel<T>(string key)
        {            
            return _Model.JsonStrToModel<T>(GetService().GetStr(key));
        }

        public static bool SetModel<T>(string key, T model)
        {
            return GetService().SetStr(key, _Model.ToJsonStr(model));
        }

        public static bool DeleteKey(string key)
        {
            return GetService().DeleteKey(key);
        }
    }
}

using Base.Models;
using Base.Services;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

//BaseWeb -> BaseApi
namespace BaseApi.Services
{
    //供 Web 應用使用。
    /// <summary>
    /// 取得 Web 語系、設定 UI 文化特性，並載入 Base 共用語系資源。
    /// </summary>
    public static class _Locale
    {
        //public static string CookieName = CookieRequestCultureProvider.DefaultCookieName;     //cookie field id for locale

        //loaded localization list, <locale, BaseResDto>
        private static Dictionary<string, BaseResDto> _brList = [];

        /*
        public static string ConfigLocale()
        {
            return _Fun.Config.Locale;
        }
        */

        /// <summary>
        /// 傳回語系代碼包含"-", 例如: zh-TW
        /// 如果非多國語則不必讀取BaseUser
        /// </summary>
        public static string GetLocale()
		{
			return _Fun.MultiLang ? _Fun.GetBaseUser().Locale : _Fun.Config.Locale;
        }

		/// <summary>
		/// 回傳不含連字號的語系代碼，例如 zhTW，供查找 XpCode 的多語欄位。
		/// </summary>
		public static string GetLocaleNoDash()
		{
            return GetLocale().Replace("-", "");
		}

		/// <summary>
		/// 依指定語系載入 BR.json，並依多語設定更新執行緒或預設執行緒的 UI 文化特性。載入失敗時回傳 false。
		/// </summary>
		/// <param name="locale">語系代碼，例如 zh-TW，用於定位語系目錄及設定 UI 文化特性。</param>
		/// <returns>設定成功時為 true，資源載入失敗時為 false。</returns>
		public static async Task<bool> SetCultureA(string locale)
        {
            //add _brList if need
            //var error = "";
            if (!_brList.Any(a => a.Key == locale))
            {
                var br = await ReadBaseResA(locale);
                if (br == null)
                {
                    _Log.Error($"_Locale.cs SetCultureA() failed, no locale ({locale})");
                    return false;
                }
                _brList.Add(locale, br);    //add first
            }

            if (_Fun.MultiLang)
            {
                //使用切換方式, 不設定default
                CultureInfo.CurrentUICulture = new CultureInfo(locale);
                //CultureInfo.CurrentCulture = new CultureInfo(locale);
            }
            else
            {
                //直接設定default culture
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(locale);    //控制 Resource(.resx)
                //CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(locale);;   //控制數字/日期格式
            }

            /*
            //set default language, after .net 4.5 ver just set DefaultThread
            //if (CultureInfo.CurrentCulture.Name != locale)
            //{
            var culture = new CultureInfo(locale);
            //CultureInfo.DefaultThreadCurrentCulture = culture;      //控制數字/日期格式
            CultureInfo.DefaultThreadCurrentUICulture = culture;    //控制 Resource(.resx)

                //Thread.CurrentThread.CurrentCulture = culture;
                //Thread.CurrentThread.CurrentUICulture = culture;
            //}
            */

            return true;
            //cookie set locale code
            //_Web.GetResponse().Cookies.Append(CookieName, locale);

            //if (_localeService != null)
            //    _localeService.SetLocale(locale);
        }

        /// <summary>
        /// get locale code
        /// </summary>
        /// <returns></returns>
        /*
        public static string GetLocaleByUser(bool dash = true)
        {
            var user = _Fun.GetBaseUser();
            var locale = (user.Locale == "") ? _Fun.Config.Locale : user.Locale;
            if (!dash) locale = locale.Replace("-", "");
            return locale;
        }
        */

        /// <summary>
        /// get locale by cookie
        /// </summary>
        /// <returns></returns>
        /*
        public static string GetLocaleByCookie()
        {
            var cookie = _Http.GetRequest().Cookies.GetValueByName(CookieName);
            return (cookie == null) ? "" : cookie.ToString()!;
        }
        */

        /// <summary>
        /// 依目前語系取用已載入的共用資源。查無對應語系時，現行實作會回傳 null。
        /// </summary>
        public static BaseResDto GetBaseRes()
        {
            var locale = GetLocale();
            var dict = _brList.FirstOrDefault(a => a.Key == locale);
            return dict.Equals(default(Dictionary<string, BaseResDto>)) 
                ? new() : dict.Value;
        }

        private static async Task<BaseResDto?> ReadBaseResA(string locale)
        {
            //error = ""; //initial
            var file = $"{_Fun2.DirWeb}locale/{locale}/BR.json";
            if (!File.Exists(file))
            {
                await _Log.ErrorRootA("no file: " + file);
                return null;
            }

            //set _br
            var br = new BaseResDto(); //initial value
            var json = _Str.ToJson((await _File.ToStrA(file))!);
            _Json.CopyToModel(json!, br);
            return br;
        }

        #region remark code
        /*         
        /// <summary>
        /// ?? get global resource, call rm.GetString(fid) when read field value
        /// </summary>
        /// <param name="fileName">resource file name, no extension</param>
        /// <returns></returns>
        public static ResourceManager GetResourceFile(string fileName)
        {
            try
            {
                return new ResourceManager(fileName + ".resx", Assembly.GetExecutingAssembly());
            }
            catch(Exception ex)
            {
                _Log.Error("_Locale.cs GetResourceFile() failed: " + ex.Message);
                return null;
            }
        }
          
        public static string ResourceStr(ResourceManager rm, string fid)
        {
            return rm.GetString(fid);
        }
        */
        #endregion

    }//class
}

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
    //for web system only
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
        /// get user locale
        /// </summary>
        /// <param name="defaultLoc">如果非多國語則傳回Config.Locale(for 大部分情形)</param>
        /// <returns></returns>
        public static string GetLocale(bool configLoc = true)
        {
            return _Fun.MultiLang ? _Fun.GetBaseUser().Locale : 
                configLoc ? _Fun.Config.Locale : "";
        }

        /// <summary>
        /// change culture
        /// </summary>
        /// <param name="locale"></param>
        /// <returns>error msg if any</returns>
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

            //set default language, after .net 4.5 ver just set DefaultThread
            //if (CultureInfo.CurrentCulture.Name != locale)
            //{
            var culture = new CultureInfo(locale);
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;

                //Thread.CurrentThread.CurrentCulture = culture;
                //Thread.CurrentThread.CurrentUICulture = culture;
            //}

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
        /// get base resource
        /// </summary>
        /// <param name="locale">default to user locale</param>
        /// <returns></returns>
        public static BaseResDto GetBaseRes()
        {
            var locale = GetLocale(true);
            var dict = _brList.FirstOrDefault(a => a.Key == locale);
            return dict.Equals(default(Dictionary<string, BaseResDto>)) 
                ? new() : dict.Value;
        }

        private static async Task<BaseResDto?> ReadBaseResA(string locale)
        {
            //error = ""; //initial
            var file = _FunApi.DirWeb + "locale/" + locale + "/BR.json";
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

using Base.Models;
using Base.Services;
using BaseApi.Services;
using Microsoft.AspNetCore.Localization;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BaseWeb.Services
{
    //for web system only
    public static class _Locale
    {
        public static string CookieName = CookieRequestCultureProvider.DefaultCookieName;     //cookie field id for locale

        //loaded localization list, <locale, BaseResDto>
        private static Dictionary<string, BaseResDto> _brList = new();

        /// <summary>
        /// change culture
        /// </summary>
        /// <param name="locale"></param>
        /// <returns>error msg if any</returns>
        public static async Task<bool> SetCultureAsync(string locale)
        {
            //add _brList if need
            //var error = "";
            if (!_brList.Any(a => a.Key == locale))
            {
                var br = await ReadBaseResAsync(locale);
                _brList.Add(locale, br);    //add first
                if (br == null)
                {
                    await _Log.ErrorAsync($"_Locale.cs SetCulture() failed, no locale ({locale})");
                    return false;
                }
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
        public static string GetLocaleByUser(bool dash = true)
        {
            var user = _Fun.GetBaseUser();
            var locale = (user.Locale == "") ? _Fun.Config.Locale : user.Locale;
            if (!dash)
                locale = locale.Replace("-", "");
            return locale;
        }

        /// <summary>
        /// get locale by cookie
        /// </summary>
        /// <returns></returns>
        public static string GetLocaleByCookie()
        {
            var cookie = _Http.GetRequest().Cookies.GetValueByName(CookieName);
            return (cookie == null) ? "" : cookie.ToString();
        }

        /// <summary>
        /// get base resource
        /// </summary>
        /// <param name="locale">default to user locale</param>
        /// <returns></returns>
        public static BaseResDto GetBaseRes(string locale = "")
        {
            if (_Str.IsEmpty(locale))
                locale = GetLocaleByUser();

            var dict = _brList.FirstOrDefault(a => a.Key == locale);
            return dict.Equals(default(Dictionary<string, BaseResDto>)) 
                ? null : dict.Value;
        }

        private static async Task<BaseResDto> ReadBaseResAsync(string locale)
        {
            //error = ""; //initial
            var file = _Web.DirWeb + "locale/" + locale + "/BR.json";
            if (!File.Exists(file))
            {
                await _Log.ErrorAsync("no file: " + file);
                return null;
            }

            //set _br
            var br = new BaseResDto(); //initial value
            var json = _Str.ToJson(await _File.ToStrAsync(file));
            _Json.CopyToModel(json, br);
            return br;
        }

        #region remark code
        /*
        /// <summary>
        /// get locale file path
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetPath(string fileName)
        {
            return _Web.DirWeb + "locale\\" + GetLocaleByUser() + "\\" + fileName;
        }

        /// <summary>
        /// frontEnd datetime string to backEnd datetime, consider different locale
        /// </summary>
        /// <returns></returns>
        public static DateTime FrontToBack(string dateStr)
        {
            //var dt = Convert.ToDateTime(dateStr).ToString(XpService.DateFormat);
            //var dt = DateTime.ParseExact(dateStr, XpService.DateFormat, CultureInfo.InvariantCulture);
            //DateTime dt;
            DateTime.TryParseExact(dateStr, BackDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt);
            return dt;
        }

        /// <summary>
        /// get locale code, ex: zh-TW
        /// also called by _Layout.cshtml
        /// </summary>
        /// <returns></returns>
        public static string GetLocale(bool dash = true)
        {
            //return Thread.CurrentThread.CurrentCulture.Name;
            var locale = (_localeService == null) ? _Fun.GetBaseUser().Locale : _localeService.GetLocale();
            if (!dash)
                locale = locale.Replace("-", "");
            return locale;
        }
        */

        /*
        /// <summary>
        /// locale code no dash
        /// 從 Code table 讀取多國語資料時會用到此 method
        /// </summary>
        /// <returns></returns>
        public static string GetLocaleNoDash()
        {
            return GetLocale().Replace("-", "");
        }

        /// <summary>
        /// set client locale
        /// </summary>
        /// <param name="locale"></param>
        public static void SetLocale(string locale)
        {
            if (_service != null)
                _service.SetLocale(locale);
        }
        */

        /// <summary>
        /// get resource for base component
        /// </summary>
        /// <param name="locale"></param>
        /// <returns></returns>
        //private static RBDto SetRB(string locale = "")
        //{
        //    if (_Str.IsEmpty(locale))
        //        locale = GetLocale();
        //    /*
        //    if (String.IsNullOrEmpty(locale))
        //    {
        //        //如果沒有session資料(未登入), 則使用預設語系
        //        //var userInfo = _Session.Read();
        //        //locale = (userInfo == null) ? _Fun.Locale : userInfo.Locale;
        //        locale = GetLocale();
        //    }
        //    */

        //    //如果語系cache已經存在, 則直接讀取
        //    var rb = _RBs.Where(a => a.Key == locale)
        //        .Select(a => a.Value)
        //        .FirstOrDefault();
        //    if (rb == null)
        //    {
        //        rb = new RBDto(); //initial value
        //        var file = _Fun.DirRoot + "Locale/" + locale + "/RB.json";
        //        if (File.Exists(file))
        //        {
        //            var json = _Str.ToJson(_File.ToStr(file));
        //            _Json.CopyToModel(json, rb);
        //        }
        //        _RBs.Add(locale, rb);
        //    }

        //    RB = rb;
        //    return rb;
        //}

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
          
        /// <summary>
        /// ??
        /// (for 前端)把DateTime轉換成為日期字串, 考慮多國語
        /// </summary>
        /// <returns></returns>
        public static string BackToFront(DateTime? dt)
        {
            return dt == null ? "" : dt.Value.ToString(DateFormatBack);
        }
        /// <summary>
        /// 把日期字串轉換成為另一種格式, 考慮多國語
        /// </summary>
        /// <returns></returns>
        public static string FormatDateStr(string dateStr)
        {
            var dt = XpService.StrToDate(dateStr);
            return dt == null ? "" : dt.ToString(XpService.DateFormatFront);
        }

        /// <summary>
        /// //??
        /// 初始化, 傳入 dateformat, 如果沒有呼叫此函數, 則使用預設格式
        /// </summary>
        /// <param name="dateFormat"></param>
        public static void Init(string dateFormatFront, string dateFormatBack)
        {
            DateFormatFront = dateFormatFront;
            DateFormatBack = dateFormatBack;
        }

        //不處理後端共用的語系問題(太複雜) !! 
        //get 語系資料 for base class
        private static Locale0Model GetLocale0(string locale, Db db = null)
        {
            var emptyDb = (db == null);
            if (emptyDb)
                db = new Db();

            var row = db.GetModel<Locale0Model>("select * from _Locale0 where Locale='" + locale + "'");
            if (emptyDb)
                db.Dispose();
            if (row == null)
            {
                _Log.Error("_Locale0 has no Locale row for " + locale);
                row = new Locale0Model();
            }
            return row;
        }

        public static string ResourceStr(ResourceManager rm, string fid)
        {
            return rm.GetString(fid);
        }
        */
        #endregion

    }//class
}

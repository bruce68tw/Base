using Base.Services;

namespace BaseWeb.Services
{
    //??
    public class LocaleCookieService : ILocale
    {
        public string GetLocale()
        {
            var cookie = _Web.GetRequest().Cookies.GetValueByName(_Web.LocaleFid);
            return (cookie == null) ? _Fun.GetBaseUser().Locale : cookie.ToString();
        }

        //get cookie for session key
        public void SetLocale(string locale)
        {
            _Web.GetResponse().Cookies.Append(_Web.LocaleFid, locale);
        }
    }
}

using Base.Services;
using BaseApi.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseWeb.Services
{
    //use OAuth, google will callback http action, so put BaseWeb
    public static class _GoogleAuth
    {

        private static bool _init = false;
        //private static string _redirectUri;
        //private static string _rootUrl;
        private static string _redirect;
        private static string _clientId;// = "1070909519480-jm0rnkt6sea0qlpnqq0b6imtkqte7ikq.apps.googleusercontent.com";
        private static string _clientSecret;// = "fVJuEf7f1GXRHOky7N0Q9WSE";

        public static void Init(string redirect, string clientId, string clientSecret)
        {
            if (_init)
                return;

            _init = true;
            //_rootUrl = _Str.AddRightSlash2(rootUrl);
            _redirect = redirect;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public static string GetAuthUrl()
        {
            //rootUrl = _Str.AddRightSlash2(rootUrl);
            
            var url = "https://accounts.google.com/o/oauth2/v2/auth";
            //var scope = "https://www.googleapis.com/auth/userinfo.profile";
            var scope = "https://www.googleapis.com/auth/userinfo.email";   //存取email資料
            //& prompt = consent
            //var redirect = _rootUrl + "signin-google";
            //redirect = _rootUrl + redirect;
            return string.Format("{0}?redirect_uri={1}&response_type=code&client_id={2}&scope={3}", url, _redirect, _clientId, scope);
        }

        //auth code to token
        public static async Task<string> CodeToToken(string code)
        {
            //redirect = _rootUrl + redirect;
            var url = "https://www.googleapis.com/oauth2/v4/token";
            var args = string.Format("code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type=authorization_code", code, _clientId, _clientSecret, _redirect);
            var json = _Str.ToJson(await _Http.GetUrlResult(url, args, false));
            return json["access_token"].ToString();

            /*
            //string param = string.Format(data, Code, client_id, client_secret, redirect_uri_encode, grant_type);
            byte[] bs = Encoding.ASCII.GetBytes(args);

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }

            using (WebResponse response = request.GetResponse())
            {
                StreamReader sr = new StreamReader(response.GetResponseStream());
                result = sr.ReadToEnd();
                sr.Close();
            }

            //get token
            var json = _Str.ToJson(result);
            return json["access_token"].ToString();
            //TokenData tokenData = JsonConvert.DeserializeObject<TokenData>(result);
            //Session["token"] = tokenData.access_token;

            //not to send Token as args for CallAPI for security reason !!
            //return RedirectToAction("CallAPI");
            */
        }

        //has email
        public static async Task<JObject> GetUser(string token)
        {
            var url = "https://www.googleapis.com/oauth2/v1/userinfo?access_token={0}";
            //var url = "https://www.googleapis.com/auth/userinfo.email?access_token={0}";
            var result = await _Http.GetUrlResult(string.Format(url, token));
            return _Str.ToJson(result);
        }

        #region remark code
        /*
        public static string GetEmail(string token)
        {
            var url = "https://www.googleapis.com/oauth2/v1/userinfo?access_token={0}";
            //url = ;
            var result = _Http.GetUrlResult(string.Format(url, token));
            return _Str.ToJson(result);
        }
        */

        /*
        /// <summary>
        /// 傳回 WebRequest html 字串
        /// </summary>
        /// <param name="code"></param>
        /// <param name="redirect">redirect action</param>
        /// <returns></returns>
        public static string CodeToTokenText2(string code)
        {
            //redirect = _rootUrl + redirect;
            var url = "https://www.googleapis.com/oauth2/v4/token";
            return string.Format(@"
<html><form id='_formToken' action='{0}' method='post'>
    <input type='hidden' name='code' value='{1}'/>
    <input type='hidden' name='client_id' value='{2}'/>
    <input type='hidden' name='client_secret' value='{3}'/>
    <input type='hidden' name='redirect_uri' value='{4}'/>
    <input type='hidden' name='grant_type' value='authorization_code'/>
</form></html>
<script>document.getElementById('_formToken').submit();</script>
", url, code, _clientId, _clientSecret, _redirectAction);
            
        }
        */


        /*
        public static JObject GetMe(string token, string connectId)
        {
            var url = "https://www.googleapis.com/plus/v1/people/me/{0}?access_token={1}";
            var result = _Http.GetUrlResult(string.Format(url, connectId, token));
            return _Str.ToJson(result);
        }
        */
        #endregion

    }//class
}

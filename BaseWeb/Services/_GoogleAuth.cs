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
        private static string _redirect;
        private static string _clientId;
        private static string _clientSecret;

        public static void Init(string redirect, string clientId, string clientSecret)
        {
            if (_init)
                return;

            _init = true;
            _redirect = redirect;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        /// <summary>
        /// get auth url for google login button url
        /// </summary>
        /// <param name="arg">google use state' for extra parameter</param>
        /// <returns></returns>
        public static string GetAuthUrl(string arg = "")
        {
            //response_type=code for call by server side !!
            var url = "https://accounts.google.com/o/oauth2/v2/auth";
            var scope = "https://www.googleapis.com/auth/userinfo.email";   //get email (userinfo.profile for user name)
            url = $"{url}?redirect_uri={_redirect}&client_id={_clientId}&scope={scope}&response_type=code";
            if (arg != "")
                url += "&state=" + arg;
            return url;
        }

        /// <summary>
        /// auth code to token
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static async Task<string> CodeToToken(string code)
        {
            var url = "https://www.googleapis.com/oauth2/v4/token";
            var args = $"code={code}&client_id={_clientId}&client_secret={_clientSecret}&redirect_uri={_redirect}&grant_type=authorization_code";
            var json = _Str.ToJson(await _Http.GetUrlResult(url, args, false));
            return json["access_token"].ToString();
        }

        /// <summary>
        /// get user info
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<JObject> GetUser(string token)
        {
            //var url = $"https://www.googleapis.com/oauth2/v1/userinfo?access_token={token}";
            var url = $"https://www.googleapis.com/oauth2/v3/userinfo?access_token={token}";
            var result = await _Http.GetUrlResult(url);
            return _Str.ToJson(result);
        }

        #region remark code
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

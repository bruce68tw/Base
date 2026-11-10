using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    //Google OAuth 回呼由使用端的 HTTP Action 接收。
    /// <summary>
    /// 提供 Google OAuth 授權網址、授權碼換取存取權杖及使用者資料查詢。
    /// </summary>
    public static class _AuthGoogle
    {
        private static bool _init = false;
        private static string _redirect = "";
        private static string _clientId = "";
        private static string _clientSecret = "";

        /// <summary>
        /// 儲存回呼網址、用戶端 ID 和密鑰。初始化成功後，後續呼叫沿用第一次設定。
        /// </summary>
        /// <param name="redirect">OAuth 回呼網址。</param>
        /// <param name="clientId">OAuth 用戶端 ID。</param>
        /// <param name="clientSecret">用於交換權杖的 OAuth 用戶端密鑰。</param>
        public static void Init(string redirect, string clientId, string clientSecret)
        {
            if (_init) return;

            _init = true;
            _redirect = redirect;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        /// <summary>
        /// 建立授權網址，可用 arg 帶入 OAuth state。
        /// </summary>
        /// <param name="arg">可選的 OAuth state 值，空字串時省略。</param>
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
        /// 使用已初始化的應用設定，向 OAuth 供應商交換存取權杖。
        /// </summary>
        /// <param name="code">OAuth 回呼取得的授權碼。</param>
        public static async Task<string> CodeToTokenA(string code)
        {
            var url = "https://www.googleapis.com/oauth2/v4/token";
            var args = $"code={code}&client_id={_clientId}&client_secret={_clientSecret}&redirect_uri={_redirect}&grant_type=authorization_code";
            var json = _Str.ToJson(await _Http.GetUrlResultA(url, args, false));
            return json!["access_token"]!.ToString();
        }

        /// <summary>
        /// 使用存取權杖查詢使用者資料並解析為 JSON 物件。
        /// </summary>
        /// <param name="token">用來向供應商查詢使用者資料的存取權杖。</param>
        public static async Task<JObject?> GetUserA(string token)
        {
            //var url = $"https://www.googleapis.com/oauth2/v1/userinfo?access_token={token}";
            var url = $"https://www.googleapis.com/oauth2/v3/userinfo?access_token={token}";
            var result = await _Http.GetUrlResultA(url);
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

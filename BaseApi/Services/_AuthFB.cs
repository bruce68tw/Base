using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    //Facebook 的 OAuth 授權流程。
    /// <summary>
    /// 提供 Facebook OAuth 授權網址、授權碼換取存取權杖及使用者資料查詢。
    /// </summary>
    public static class _AuthFB
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
            var url = "https://www.facebook.com/v13.0/dialog/oauth";
            url = $"{url}?redirect_uri={_redirect}&client_id={_clientId}&response_type=code&auth_type=reauthenticate";
            if (arg != "")
                url += "&state=" + arg;
            return url;
        }

        //以授權碼交換存取權杖。
        /// <summary>
        /// 使用已初始化的應用設定，向 OAuth 供應商交換存取權杖。
        /// </summary>
        /// <param name="code">OAuth 回呼取得的授權碼。</param>
        public static async Task<string> CodeToTokenA(string code)
        {
            var url = "https://graph.facebook.com/v13.0/oauth/access_token";
            var args = $"code={code}&client_id={_clientId}&client_secret={_clientSecret}&redirect_uri={_redirect}&grant_type=authorization_code";
            var json = _Str.ToJson(await _Http.GetUrlResultA(url, args, false));
            return json!["access_token"]!.ToString();
        }

        //使用者資料包含電子郵件欄位。
        /// <summary>
        /// 使用存取權杖查詢使用者資料並解析為 JSON 物件。
        /// </summary>
        /// <param name="token">用來向供應商查詢使用者資料的存取權杖。</param>
        public static async Task<JObject?> GetUserA(string token)
        {
            var url = "https://graph.facebook.com/v2.3/me";
            //url = $"{url}?fields=name,email&access_token={token}";
            url = $"{url}?fields=email&access_token={token}";
            var result = await _Http.GetUrlResultA(url);
            return _Str.ToJson(result);
        }

    }//class
}

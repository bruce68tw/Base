﻿using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    //Facebook OAuth
    public static class _AuthFB
    {
        private static bool _init = false;
        private static string _redirect = "";
        private static string _clientId = "";
        private static string _clientSecret = "";

        public static void Init(string redirect, string clientId, string clientSecret)
        {
            if (_init) return;

            _init = true;
            _redirect = redirect;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        /// <summary>
        /// get auth url
        /// </summary>
        /// <param name="arg">use state' for extra parameter</param>
        /// <returns></returns>
        public static string GetAuthUrl(string arg = "")
        {
            //response_type=code for call by server side !!
            var url = "https://www.facebook.com/v13.0/dialog/oauth";
            url = $"{url}?redirect_uri={_redirect}&client_id={_clientId}&response_type=code&auth_type=reauthenticate";
            if (arg != "")
                url += "&state=" + arg;
            return url;
        }

        //auth code to token
        public static async Task<string> CodeToTokenA(string code)
        {
            var url = "https://graph.facebook.com/v13.0/oauth/access_token";
            var args = $"code={code}&client_id={_clientId}&client_secret={_clientSecret}&redirect_uri={_redirect}&grant_type=authorization_code";
            var json = _Str.ToJson(await _Http.GetUrlResultA(url, args, false));
            return json!["access_token"]!.ToString();
        }

        //email in it
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

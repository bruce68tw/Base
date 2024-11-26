using Base.Models;
using Base.Services;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BaseApi.Services
{

    public static class _Login
    {
        /// <summary>
        /// 使用 View Object 登入
        /// </summary>
        /// <param name="vo">login view object</param>
        /// <param name="encodePwd">是否加密密碼欄位</param>
        /// <returns></returns>
        public static async Task<bool> LoginByVoA(LoginVo vo, bool encodePwd)
        {
            //reset UI msg first
            vo.AccountMsg = "";
            vo.PwdMsg = "";
            vo.ErrorMsg = "";

            #region 1.check input account & password
            var hasPwd = _Str.NotEmpty(vo.Pwd);
            if (_Str.IsEmpty(vo.Account))
            {
                vo.AccountMsg = "field is required.";
                goto lab_error;
            }
            /*
            if (_Str.IsEmpty(vo.Pwd))
            {
                vo.PwdMsg = "field is required.";
                goto lab_error;
            }
            */
            #endregion

            #region 2.check DB password & get user info
            var sql = @"
select u.Id as UserId, u.Name as UserName, u.Pwd,
    u.DeptId, d.Name as DeptName
from dbo.[User] u
join dbo.Dept d on u.DeptId=d.Id
where u.Account=@Account
";
            var status = false;
            var row = await _Db.GetRowA(sql, ["Account", vo.Account]);
            if (row != null)
            {
                var dbPwd = row["Pwd"]!.ToString();
                if (hasPwd)
                {
                    var inputPwd = encodePwd ? _Str.Md5(vo.Pwd) : vo.Pwd;   //encode if need
                    status = (inputPwd == dbPwd);
                }
                else
                {
                    status = (dbPwd == "");
                }
            }

            if (!status)
            {
                vo.AccountMsg = "input wrong.";
                goto lab_error;
            }
            #endregion

            #region 3.set base user info
            var key = _Str.NewId(15);
            var userId = row!["UserId"]!.ToString();
            var userInfo = new BaseUserDto()
            {
                UserId = userId,
                UserName = row["UserName"]!.ToString(),
                HasPwd = hasPwd,
                DeptId = row["DeptId"]!.ToString(),
                DeptName = row["DeptName"]!.ToString(),
                Locale = _Fun.Config.Locale,
                ProgAuthStrs = await _Auth.GetAuthStrsA(userId),
                //IsLogin = true,
            };
            #endregion

            //write cache server for base user info, key值加上IP
            _Cache.SetModel(key + _Http.GetIp(false), _Fun.FidBaseUser, userInfo);

            //寫入cookie
            _Http.SetCookie(_Fun.FidClientKey, key);

            //5.redirect if need
            //var url = _Str.IsEmpty(vo.FromUrl) ? "/Home/Index" : vo.FromUrl;
            //return Redirect(url);
            return true;

        lab_error:
            //return View(vo);
            return false;
        }

        /// <summary>
        /// 使用 UserId 登入(用在手機 app), 如果IP不同則進行2FA
        /// </summary>
        /// <param name="userId">user Id</param>
        /// <returns>1(成功),0(IP不同),-1(失敗, 帳號不存在)</returns>
        public static async Task<int> LoginByUidA(string userId, Db? db = null)
        {
            const int Error = -1;
            const int Ok = 1;
            const int IpWrong = 0;

            //1.check empty
            var result = Error;
            if (_Str.IsEmpty(userId)) return result;

            //2.compare Ip
            var newDb = _Db.CheckOpenDb(ref db);
            var sql = "select Ip from dbo.[UserApp] where Id=@Id";
            var row = await _Db.GetRowA(sql, ["Id", userId], db);
            if (row == null) goto lab_exit;

            //check Ip
            result = (_Http.GetIp(true) == row["Ip"]!.ToString()) 
                ? Ok : IpWrong;

        lab_exit:
            await _Db.CheckCloseDbA(db!, newDb);
            return result;
        }

        /// <summary>
        /// get JWT key(256), 使用 IP
        /// </summary>
        /// <returns></returns>
        public static SymmetricSecurityKey GetJwtKey()
        {
            return new(Encoding.UTF8.GetBytes(_Str.PreZero(32, _Fun.JwtKey, true)));
        }

        /// <summary>
        /// 傳回 JWT 授權字串
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static string GetJwtAuthStr(string userId)
        {
            var token = new JwtSecurityToken(
                claims:
                [
                    new Claim(ClaimTypes.Name, userId),
                ],
                signingCredentials: new SigningCredentials(
                    GetJwtKey(),
                    SecurityAlgorithms.HmacSha256
                ),
                expires: DateTime.Now.AddMinutes(_Fun.TimeOut)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static TokenValidationParameters GetJwtArg()
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,    //是否驗證密鑰
                IssuerSigningKey = GetJwtKey(),     //SecurityKey
                ValidateLifetime = true,            //是否驗證超時  當設置exp和nbf時有效 
                ClockSkew = TimeSpan.FromMinutes(_Fun.TimeOut),   //設置過期時間，如無設定則預設為5分鐘
                ValidateAudience = false,   //default true !!
                ValidateIssuer = false,     //default true !!

                //ValidateIssuer = false,   //簽發者
                //ValidateAudience = false, //接收者
                //ValidAudience = "http://localhost:49999",//Audience
                //ValidIssuer = "http://localhost:49998",//Issuer，這兩項和登入時頒發的一致
                //緩衝過期時間，總的有效時間等於這個時間加上jwt的過期時間，預設為5分鐘                                                                                                            //注意這是緩衝過期時間，總的有效時間等於這個時間加上jwt的過期時間，如果不配置，默認是5分鐘
            };
        }

        /// <summary>
        /// 記錄登入成功 (Login table)
        /// </summary>
        /// <param name="account"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static async Task LogOkA(string account, Db? db = null)
        {
            var newDb = _Db.CheckOpenDb(ref db);
            var args = new List<object>() { "Account", account };
            var count = await db!.ExecSqlA($@"
update dbo.Login set LoginStatus=1, LoginTime=getDate(), FailCount=0
where Account=@Account
", args);
            if (count == 0)
            {
                await db.ExecSqlA($@"
insert into dbo.Login(Account, LoginStatus, LoginTime, FailCount) values (
@Account, 1, getDate(), 0
)", args);
            }
            await _Db.CheckCloseDbA(db!, newDb);
        }

        /// <summary>
        /// 記錄登入失敗
        /// </summary>
        /// <param name="account"></param>
        /// <param name="db"></param>
        /// <returns>false表示失敗次數超過 _Fun.MaxLoginFail</returns>
        public static async Task<bool> LogFailA(string account, Db? db = null)
        {
            var newDb = _Db.CheckOpenDb(ref db);
            var args = new List<object>() { "Account", account };
            var failCount = await db!.GetIntA(@"
select FailCount from dbo.Login
where Account=@Account
", args);
            bool result;
            if (failCount == null)
            {
                await db.ExecSqlA(@"
insert into dbo.Login(Account, LoginStatus, LoginTime, FailCount) values (
@Account, 1, getDate(), 1
)", args);
                result = true;
            }
            else if (failCount < _Fun.MaxLoginFail)
            {
                await db!.ExecSqlA(@"
update dbo.Login set LoginStatus=0, LoginTime=getDate(), FailCount=FailCount+1
where Account=@Account
", args);
                result = true;
            }
            else
            {
                await db!.ExecSqlA(@"
update dbo.[User] set IsLock=1
where Account=@Account
", args);
                result = false;
            }

            await _Db.CheckCloseDbA(db!, newDb);
            return result;
        }

    }//class
}

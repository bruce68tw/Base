using Base.Enums;
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

    /// <summary>
    /// 處理帳密登入與登入紀錄，並提供 JWT 簽發及驗證參數。
    /// </summary>
    public static class _Login
    {
        /// <summary>
        /// 使用 View Object 登入
        /// </summary>
        /// <param name="vo">登入輸入，方法會清除並依結果更新其中的訊息欄位。</param>
        /// <param name="sqlFindUser">查詢使用者的 SQL，接收 Account 參數。空字串時使用內建查詢。</param>
        /// <param name="encodePwd">密碼模式下，是否先以 MD5 處理輸入密碼再與資料庫值比對。</param>
        /// <param name="extCol">第一個額外值，非空時寫入使用者資料的 ExtCol，並加入預設 SQL 的 ExtCol 選取項。</param>
        /// <param name="extCol2">第二個額外值，非空時寫入使用者資料的 ExtCol2，並加入預設 SQL 的 ExtCol2 選取項。</param>
        /// <param name="extCol3">todo:待確認 預設 SQL 目前仍使用 ExtCol2 別名。非空時會寫入使用者資料的 ExtCol3。</param>
        public static async Task<bool> LoginByVoA(LoginVo vo, string sqlFindUser = "", bool encodePwd = true, 
            string extCol = "", string extCol2 = "", string extCol3 = "")
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
            //讀取額外欄位 if need
            var hasExtCol = !string.IsNullOrEmpty(extCol);
            var hasExtCol2 = !string.IsNullOrEmpty(extCol2);
            var hasExtCol3 = !string.IsNullOrEmpty(extCol3);
            var extColSql = "";
            if (hasExtCol)
                extColSql = $", ExtCol={AddQuote(extCol)}";
            if (hasExtCol2)
                extColSql += $", ExtCol2={AddQuote(extCol2)}";
            if (hasExtCol3)
                extColSql += $", ExtCol2={AddQuote(extCol3)}";

            if (string.IsNullOrEmpty(sqlFindUser))
                sqlFindUser = $@"
select u.Id as UserId, u.Name as UserName, u.Pwd,
    u.DeptId, d.Name as DeptName{extColSql}
from dbo.XpUser u
left join dbo.XpDept d on u.DeptId=d.Id
where u.Account=@Account
";
            var status = false;
            var row = await _Db.GetRowA(sqlFindUser, ["Account", vo.Account]);
            if (row != null)
            {
                #region case AD
                if (_Fun.Config.LoginType == LoginTypeEstr.Ad)
                {
                    if (!string.IsNullOrEmpty(_Fun.Config.AdServer))
                    {
                        var user = await _Ldap.LoginA(_Fun.Config.AdServer, vo.Account, vo.Pwd);
                        if (user != null)
                        {
                            status = true;
                        }
                    }
                }
                #endregion

                #region case Pwd
                else if (_Fun.Config.LoginType == LoginTypeEstr.Pwd)
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
                #endregion
            }

            if (!status)
            {
                vo.AccountMsg = "input wrong.";
                goto lab_error;
            }
            #endregion

            #region 3.set base user info
            var newKey = _Str.NewId(15);
            var userId = row!["UserId"]!.ToString();
            var userInfo = new BaseUserDto()
            {
                UserId = userId,
                UserName = row["UserName"]!.ToString(),
                DeptId = _Json.NullToEmpty(row, "DeptId"),
                DeptName = _Json.NullToEmpty(row, "DeptName"),
                HasPwd = hasPwd,
                Locale = _Fun.Config.Locale,
                ProgAuthStrs = await _Auth.GetAuthStrA(userId),
                //IsLogin = true,
            };
            //加上 ExtCol if need
            if (hasExtCol)
                userInfo.ExtCol = extCol;
            if (hasExtCol2)
                userInfo.ExtCol2 = extCol2;
            if (hasExtCol3)
                userInfo.ExtCol3 = extCol3;
            #endregion

            //write cache server for base user info, key值加上IP
            _Cache.SetModel(_Http.UserKeyIp(newKey), _Fun.FidBaseUser, userInfo);

            //寫入cookie
            _Http.SetCookie(_Fun.FidClientKey, newKey);

            //5.redirect if need
            //var url = _Str.IsEmpty(vo.FromUrl) ? "/Home/Index" : vo.FromUrl;
            //return Redirect(url);
            return true;

        lab_error:
            //return View(vo);
            return false;
        }

        //字串加上單引號 if need
        private static string AddQuote(string value)
        {
            if (!value.Contains('\''))
                value = $"'{value}'";
            return value;
        }

        /// <summary>
        /// 使用 UserId 登入(用在手機 app), 如果IP不同則進行2FA
        /// </summary>
        /// <param name="userId">要在 UserApp 查詢並比對來源 IP 的使用者 ID。</param>
        /// <param name="db">可沿用的資料庫物件，省略時由方法開啟。</param>
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
        /// 使用共用設定的 JwtKey 建立對稱簽章金鑰。
        /// </summary>
        public static SymmetricSecurityKey GetJwtKey()
        {
            return new(Encoding.UTF8.GetBytes(_Str.PreZero(32, _Fun.JwtKey, true)));
        }

        /// <summary>
        /// 傳回 JWT 授權字串
        /// 簽發以使用者 ID 作為 Name 宣告的 JWT，期限依應用的逾時設定。
        /// </summary>
        /// <param name="userId">寫入 JWT Name 宣告的使用者 ID。</param>
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

        /// <summary>
        /// 建立驗證 JWT 簽章金鑰與有效期限的參數。現行設定停用發行者和受眾驗證。
        /// </summary>
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
        /// 記錄登入成功，更新 Login 資料表並重設失敗次數。
        /// </summary>
        /// <param name="account">要記錄登入成功的帳號。</param>
        /// <param name="db">可沿用的資料庫物件，省略時由方法開啟。</param>
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
        /// <param name="account">要記錄登入失敗的帳號。</param>
        /// <param name="db">可沿用的資料庫物件，省略時由方法開啟。</param>
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

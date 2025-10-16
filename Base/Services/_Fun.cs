using Base.Enums;
using Base.Interfaces;
using Base.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Base.Services
{
    //global function
#pragma warning disable CA2211 // 非常數欄位不應可見
    public static class _Fun
    {
        #region constant
        //hidden input for CRSF
        //public const string HideKey = "_hideKey";

        //crud update/view for AuthType=Row only in xxxEdit.cs
        public const string FidUser = "_userId";
        public const string FidDept = "_deptId";
        public const string FidErrorMsg = "_ErrorMsg";  //與 ResultDto.cs 一致, 用於錯誤訊息傳回前端

        //auth client key, for cookie、JWT
        public const string FidClientKey = "_jwt";

        //對應 _BR.js(多國語) 欄位
        public const string FidNoAuthUser = "NoAuthUser";  //您只能存取個人資料，請聯絡管理者。
        public const string FidNoAuthDept = "NoAuthDept";  //您只能存取同部門資料，請聯絡管理者。
        public const string FidNoAuthProg = "NoAuthProg";  //您沒有此功能的權限，請聯絡管理者。
        public const string FidNotLogin = "NotLogin";      //您尚未登入系統。

        //session timeout(or not login), map to _BR.js
        public const string FidTimeOut = "TimeOut";

        //for CrudEditSvc, 多個檔案使用
        public const string FidRows = "_rows";          //rows fid for CrudEdit
        public const string FidChilds = "_childs";      //childs fid for CrudEdit
        public const string FidDeletes = "_deletes";    //delete key string list

        public const string FidBaseUser = "_BaseUser";         //base user info
        //public const string ProgAuthStrs = "_ProgAuthStrs"; //program autu string list

        //c# datetime format, when js send to c#, will match to _fun.MmDtFmt
        public const string CsDtFmt = "yyyy/MM/dd HH:mm:ss";
        public const string CsDtFmt2 = "yyyy/MM/dd HH:mm";
        public const string CsDateFmt = "yyyy/MM/dd";

        //carrier
        public const string TextCarrier = "\r\n";     //for string
        public const string HtmlCarrier = "<br>";     //for html

        //default pagin rows
        public const int PageRows = 10;

        //default auto Id length
        public const int AutoIdShort = 6;
        public const int AutoIdMid = 10;
        public const int AutoIdLong = 22;

        //錯誤處理方式:(1)for User Msg (2)聯絡管理者 (3)無訊息,傳回空值 ex讀取資料 (4)
        //indicate error
        public const string Error = "E";        //在程式判斷, 避免hard code
        public const string PreError = "E:";
        public const string PreBrError = "B:";  //_BR code error

		//default horizontal columns: view cols for layout(row div, label=2, input=3)(horizontal) 
		public const string DefHoriCols = "2,3";
		public static List<int> DefHoriColList = _Str.ToIntList(DefHoriCols);

        //directory tail seperator
        public static char DirSep = Path.DirectorySeparatorChar;

        //class name for hide element in RWD phone, HideRwd -> ClsHideRwd
        public static string ClsHideRwd = "x-hide-rwd";
        #endregion

        #region variables PG can modify
        /// <summary>
        /// session timeout(unit: minutes)
        /// </summary>
        public static int TimeOut = 600;

        //max login fail count
        public static int MaxLoginFail = 3;

        //max export rows count, also for 非分頁功能讀取資料
        public static int MaxExportCount = 3000;

        //密碼強度: 0(無限制), 1(中等:英數字), 2(強:大小寫英文,數字,特殊符號,長度10以上)
        public static int PwdStrongLevel = 0;

        //crud read for AuthType=Row only in xxxRead.cs/xxxR.cs
        public static string UserEqual = "u.Id='{0}'";
        public static string DeptEqual = "u.DeptId='{0}'";

        //AES & JWT key, be set when initial
        //public static string AesKey = "YourAesKey";
        public static string JwtKey = "YourJwtKey";

        public static string SystemError = "System Error, Please Contact Administrator.";

        //for 資安 script, style 使用 inline
        public static string Nonce = "";
        #endregion

        #region input parameters
        //is devironment or not
        public static bool IsDev;

        //執行模式, Development/Production, 會使用這個字串來讀取固定檔名
        public static string RunModeName = "";

        //null! 表示在使用前設定
        public static IServiceProvider DiBox = null!;

        //database type
        public static DbTypeEnum DbType = DbTypeEnum.MSSql;

        //program auth type
        public static AuthTypeEnum AuthType = AuthTypeEnum.None;

        /// <summary>
        /// 是否使用多國語
        /// </summary>
        public static bool MultiLang = false;
        #endregion

        #region dir varibles
        //ap physical path, has right slash
        public static string DirRoot = _Str.GetLeft(AppDomain.CurrentDomain.BaseDirectory, "bin" + DirSep);

        //_data folder
        public static string DirData = DirRoot + "_data" + DirSep;

        //image folder
        public static string DirImage = DirRoot + "_image" + DirSep;
        public static string NoImagePath = DirImage + "NoImage.jpg";

        //temp folder
        public static string DirTemp = DirRoot + "_temp" + DirSep;
        #endregion

        #region Db variables
        //datetime format for read/write db
        public static string DbDtFmt = "";
        public static string DbDateFmt = "";

        //for read page rows
        public static string ReadPageSql = "";

        //for delete rows
        public static string DeleteRowsSql = "";
        #endregion

        #region others variables
        //from config file
        public static ConfigDto Config = null!;

        public static SmtpDto? Smtp;
        #endregion

        /*
        //constructor
        static _Fun()
        {
        }
        */

        /// <summary>
        /// initial db environment for Ap with db function !!
        /// </summary>
        /// <param name="isDev">is devironment or not</param>
        /// <param name="diBox"></param>
        /// <param name="dbType">資料庫種類</param>
        /// <param name="authType">權限種類</param>
        /// <param name="multiLang">是否使用多國語</param>
        /// <returns>error msg if any</returns>
        public static string Init(bool isDev, IServiceProvider? diBox, DbTypeEnum dbType, 
            AuthTypeEnum authType, bool multiLang)
        {
            #region set instance variables
			IsDev = isDev;
            RunModeName = isDev ? "Development" : "Production";
            DiBox = diBox;
            DbType = dbType;
            //AuthType = (Config.LoginType == LoginTypeEstr.None) ? AuthTypeEnum.None : authType; //無登入必為無權限 !!
            AuthType = authType;
            MultiLang = multiLang;

            Config!.HtmlImageUrl = _Str.AddSlash(Config.HtmlImageUrl);
            //Nonce = _Str.NewId();

            //解密敏感組態資料 if need
            if (Config.Encode)
            {
                Config.Db = _Str.DecodeByFile(Config.Db).Replace("\\\\","\\");    //config的\到字串會變\\
                Config.Smtp = _Str.DecodeByFile(Config.Smtp);
                Config.Redis = _Str.DecodeByFile(Config.Redis);
                Config.OtpAuthKey = _Str.DecodeByFile(Config.OtpAuthKey);
                Config.SmsAccount = _Str.DecodeByFile(Config.SmsAccount);
                Config.SmsPwd = _Str.DecodeByFile(Config.SmsPwd);
            }
            #endregion

            #region set smtp
            var smtp = Config.Smtp;
            if (!string.IsNullOrEmpty(smtp))
            {
                try
                {
                    var cols = smtp.Split(',');
                    Smtp = new SmtpDto()
                    {
                        Host = cols[0],
                        Port = int.Parse(cols[1]),
                        Ssl = bool.Parse(cols[2]),
                        Id = cols[3],
                        Pwd = cols[4],
                        FromEmail = cols[5],
                        FromName = cols[6]
                    };
                }
                catch
                {
                    return "config Smtp is wrong(7 cols): Host,Port,Ssl,Id,Pwd,FromEmail,FromName";
                }
            }
            #endregion

            #region set DB variables
            //0:select, 1:order by, 2:start row(base 0), 3:rows count
            switch (dbType)
            {
                case DbTypeEnum.MSSql:
                    DbDtFmt = "yyyy-MM-dd HH:mm:ss";
                    DbDateFmt = "yyyy-MM-dd";

                    //for sql 2012, more easy
                    //note: offset/fetch not sql argument
                    ReadPageSql = @"
select {0} {1}
offset {2} rows fetch next {3} rows only
";
                    DeleteRowsSql = "delete {0} where {1} in ({2})";    
                    break;

                case DbTypeEnum.MySql:
                    DbDtFmt = "YYYY-MM-DD HH:mm:SS";
                    DbDateFmt = "YYYY-MM-DD";

                    ReadPageSql = @"
select {0} {1}
limit {2},{3}
";
                    //TODO: set delete sql for MySql
                    //DeleteRowsSql = 
                    break;

                case DbTypeEnum.Oracle:
                    DbDtFmt = "YYYY/MM/DD HH24:MI:SS";
                    DbDateFmt = "YYYY/MM/DD";

                    //for Oracle 12c after(same as mssql)
                    ReadPageSql = @"
Select {0} {1}
Offset {2} Rows Fetch Next {3} Rows Only
";
                    //TODO: set delete sql for Oracle
                    //DeleteRowsSql = 
                    break;
            }
            #endregion

            //case of ok
            return "";
        }

        //get current userId
        public static string Dir(string folder, bool tailSep = true)
        {
            return DirRoot + folder + (tailSep ? DirSep : "");
        }

        //get current userId
        public static string UserId()
        {
            return GetBaseUser().UserId;
        }

        public static string UserName()
        {
            return GetBaseUser().UserName;
        }

        public static bool HasPwd()
        {
            return GetBaseUser().HasPwd;
        }

        public static string DeptId()
        {
            return GetBaseUser().DeptId;
        }

        public static bool IsAuthRowAndLogin()
        {
            return IsAuthTypeRow() && IsNeedLogin();
        }

        //check is AuthType=Row
        public static bool IsAuthTypeRow()
        {
            return (AuthType == AuthTypeEnum.Row);
        }

        //是否需要登入
        public static bool IsNeedLogin()
        {
            return (Config.LoginType != LoginTypeEstr.None);
        }

        /*
        public static string GetBrError(string msg)
        {
            return IsError(msg)
                ? msg.Substring(PreError.Length)
                : "";
        }

        public static bool IsError(string msg)
        {
            var len = PreError.Length;
            return _Str.NotEmpty(msg) &&
                msg.Length >= len &&
                msg.Substring(0, len) == PreError;
        }
        */

        /// <summary>
        /// get base user info for base component
        /// 此為session等級資料, 無法以靜態變數常駐
        /// </summary>
        /// <returns>BaseUserDto(not null)</returns>
        public static BaseUserDto GetBaseUser()
        {
            var service = (IBaseUserSvc)DiBox!.GetService(typeof(IBaseUserSvc))!;
            return service.GetData() ?? new BaseUserDto();
        }

        public static void Except(string error = "")
        {
            throw new Exception(_Str.EmptyToValue(error, SystemError));
        }

        public static string GetHtmlImageUrl(string subPath)
        {
            return $"{Config!.HtmlImageUrl}{subPath}";
        }

    } //class
    #pragma warning restore CA2211 // 非常數欄位不應可見
}

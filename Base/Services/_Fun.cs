using Base.Enums;
using Base.Interfaces;
using Base.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Base.Services
{
#pragma warning disable CA2211 // 非常數欄位不應可見
    /// <summary>
    /// Base 的全域設定、共用鍵與初始化入口；狀態由整個處理程序共用，非請求或租戶隔離。
    /// </summary>
    public static class _Fun
    {
        #region constant
        //public const string HideKey = "_hideKey";

        /// <summary>
        /// 資料範圍檢查使用的使用者欄位鍵。
        /// </summary>
        public const string FidUser = "_userId";
        /// <summary>
        /// 資料範圍檢查使用的部門欄位鍵。
        /// </summary>
        public const string FidDept = "_deptId";
        /// <summary>
        /// JSON 錯誤訊息欄位鍵，對應 ResultDto._ErrorMsg。
        /// </summary>
        public const string FidErrorMsg = "_ErrorMsg";  //與 ResultDto.cs 一致, 用於錯誤訊息傳回前端

        /// <summary>
        /// Cookie 或 JWT 用戶端鍵名。
        /// </summary>
        public const string FidClientKey = "_jwt";

        /// <summary>
        /// 個人資料無權限的語系訊息鍵。
        /// </summary>
        public const string FidNoAuthUser = "NoAuthUser";  //您只能存取個人資料，請聯絡管理者。
        /// <summary>
        /// 部門資料無權限的語系訊息鍵。
        /// </summary>
        public const string FidNoAuthDept = "NoAuthDept";  //您只能存取同部門資料，請聯絡管理者。
        /// <summary>
        /// 作業無權限的語系訊息鍵。
        /// </summary>
        public const string FidNoAuthProg = "NoAuthProg";  //您沒有此功能的權限，請聯絡管理者。
        /// <summary>
        /// 尚未登入的語系訊息鍵。
        /// </summary>
        public const string FidNotLogin = "NotLogin";      //您尚未登入系統。
        /// <summary>
        /// 唯一鍵重複的語系訊息鍵。
        /// </summary>
        public const string FidUniqueError = "UniqueError"; //唯一鍵重複錯誤

        /// <summary>
        /// 逾時或未登入的語系訊息鍵。
        /// </summary>
        public const string FidTimeOut = "TimeOut";

        /// <summary>
        /// CRUD JSON 的資料列陣列鍵。
        /// </summary>
        public const string FidRows = "_rows";          //rows fid for CrudEdit
        /// <summary>
        /// 單一子項目鍵的共用常數。
        /// </summary>
        public const string FidChild = "_child";        //child fid for WordSetSvc
        /// <summary>
        /// CRUD JSON 的子表陣列鍵。
        /// </summary>
        public const string FidChilds = "_childs";      //childs fid for CrudEdit、WordSetSvc
        /// <summary>
        /// CRUD JSON 的待刪鍵清單欄位。
        /// </summary>
        public const string FidDeletes = "_deletes";    //delete key string list

        /// <summary>
        /// 目前使用者資料的共用儲存鍵。
        /// </summary>
        public const string FidBaseUser = "_BaseUser";  //base user info
        /// <summary>
        /// 前端全選欄位的共用鍵。
        /// </summary>
        public const string FidCheck0 = "_check0";      //對應 _icheck.FidCheck0
        //public const string ProgAuthStrs = "_ProgAuthStrs"; //program autu string list

        /// <summary>
        /// 共用含秒日期時間格式 yyyy/MM/dd HH:mm:ss。
        /// </summary>
        public const string CsDtFmt = "yyyy/MM/dd HH:mm:ss";
        /// <summary>
        /// 共用不含秒日期時間格式 yyyy/MM/dd HH:mm。
        /// </summary>
        public const string CsDtFmt2 = "yyyy/MM/dd HH:mm";
        /// <summary>
        /// 共用日期格式 yyyy/MM/dd。
        /// </summary>
        public const string CsDateFmt = "yyyy/MM/dd";

        /// <summary>
        /// 合併欄位後加解密使用的分隔符 @@。
        /// </summary>
        public const string ColSep = "@@";

        //public const string TextCarrier = "\r\n";     //for string
        /// <summary>
        /// 平台預設文字換行字元。
        /// </summary>
        public static string TextCarrier = Environment.NewLine;     //for 跨平台
        /// <summary>
        /// HTML 換行字串。
        /// </summary>
        public const string HtmlCarrier = "<br>";     //for html

        //public const int PageRows = 10;

        /// <summary>
        /// 短自動識別碼的長度 6。
        /// </summary>
        public const int AutoIdShort = 6;
        /// <summary>
        /// 中自動識別碼的長度 10。
        /// </summary>
        public const int AutoIdMid = 10;
        /// <summary>
        /// 長自動識別碼的長度 22。
        /// </summary>
        public const int AutoIdLong = 22;

        /// <summary>
        /// 共用錯誤代碼 E。
        /// </summary>
        public const string Error = "E";        //在程式判斷, 避免hard code
        /// <summary>
        /// 一般錯誤文字的 E: 前綴。
        /// </summary>
        public const string PreError = "E:";
        /// <summary>
        /// 語系錯誤鍵的 B: 前綴。
        /// </summary>
        public const string PreBrError = "B:";  //_BR code error

		/// <summary>
		/// 水平表單預設標籤與輸入格線寬度 2,3。
		/// </summary>
		public const string DefHoriCols = "2,3";
		/// <summary>
		/// 由 DefHoriCols 初始建立的寬度清單，不會隨字串變更自動重算。
		/// </summary>
		public static List<int> DefHoriColList = _Str.ToIntList(DefHoriCols);

        /// <summary>
        /// 目前平台的目錄分隔字元。
        /// </summary>
        public static char DirSep = Path.DirectorySeparatorChar;

        /// <summary>
        /// 行動版隱藏元素的 CSS 類別名稱。
        /// </summary>
        public static string ClsHideRwd = "x-hide-rwd";
        #endregion

        #region variables PG can modify
        /// <summary>
        /// 建立者與建立時間的預設欄位名稱。
        /// </summary>
        public static string[] Create2 = ["Creator", "Created"];

        /// <summary>
        /// 逾時設定，單位分鐘；記憶體快取滑動期限使用此值。
        /// </summary>
        public static int TimeOut = 600;

        /// <summary>
        /// 允許的登入失敗次數上限設定。
        /// </summary>
        public static int MaxLoginFail = 3;

        /// <summary>
        /// 匯出與部分非分頁查詢的預設筆數上限。
        /// </summary>
        public static int MaxExportCount = 3000;

        /// <summary>
        /// 密碼強度：0 不限制、1 要求英數，其他值使用較強規則。
        /// </summary>
        public static int PwdStrongLevel = 0;

        /// <summary>
        /// 個人範圍 SQL 條件格式，{0} 為使用者 Id。
        /// </summary>
        public static string UserEqual = "u.Id='{0}'";
        /// <summary>
        /// 部門範圍 SQL 條件格式，{0} 為部門 Id。
        /// </summary>
        public static string DeptEqual = "u.DeptId='{0}'";

        //public static string AesKey = "YourAesKey";
        /// <summary>
        /// 應用可設定的 JWT 金鑰欄位，初值為佔位字串。
        /// </summary>
        public static string JwtKey = "YourJwtKey";

        /// <summary>
        /// 對外的一般系統錯誤文字。
        /// </summary>
        public static string SystemError = "System Error, Please Contact Administrator.";

        //public static string Nonce = "";

        /// <summary>
        /// 簽核退回第一關的流程設定旗標。
        /// </summary>
        public static bool FlowBackToFirst = true;

        /// <summary>
        /// 表示全部部門的部門識別碼設定。
        /// </summary>
        public static string DeptAll = "";
        /// <summary>
        /// 表示所有人角色的識別碼，空字串表示未指定。
        /// </summary>
        public static string RoleAll = "";

        /// <summary>
        /// 是否啟用多語系處理，由 Init 設定。
        /// </summary>
        public static bool MultiLang = false;

        /// <summary>
        /// 可設定的語系清單，初值為繁中、英文與簡中。
        /// </summary>
        public static string[] Locales = ["zh-TW", "en-US", "zh-CN"];

        /// <summary>
        /// 預設語系欄位，初值為 en-US，與 Config.Locale 分開。
        /// </summary>
        public static string DefaultLocale = "en-US";

        /// <summary>
        /// 是否以部門角色關聯處理權限。
        /// </summary>
        public static bool UseDeptRole = false;

        /// <summary>
        /// 是否使用使用者下一位簽核者設定的流程旗標。
        /// </summary>
        public static bool UseNextSigner = false;
        #endregion

        #region input parameters
        /// <summary>
        /// 是否為開發環境，由 Init 設定。
        /// </summary>
        public static bool IsDev;

        /// <summary>
        /// 由 Init 設為 Development 或 Production。
        /// </summary>
        public static string RunModeName = "";

        /// <summary>
        /// 共用服務解析器；DB、使用者與快取功能須提供所需 DI 服務。
        /// </summary>
        public static IServiceProvider DiBox = null!;

        /// <summary>
        /// 目前資料庫種類，由 Init 決定 SQL 方言設定。
        /// </summary>
        public static DbTypeEnum DbType = DbTypeEnum.MSSql;

        /// <summary>
        /// 目前授權層級，初值為 None。
        /// </summary>
        public static AuthTypeEnum AuthType = AuthTypeEnum.None;
        #endregion

        #region dir varibles
        /// <summary>
        /// 從應用基底目錄與 bin 分隔位置推導的根路徑，不是目前工作目錄。
        /// </summary>
        public static string DirRoot = _Str.GetLeft(AppDomain.CurrentDomain.BaseDirectory, "bin" + DirSep);

        /// <summary>
        /// 初始化靜態欄位時由 DirRoot 組成的 _data 路徑。
        /// </summary>
        public static string DirData = DirRoot + "_data" + DirSep;

        /// <summary>
        /// 初始化靜態欄位時由 DirRoot 組成的 _image 路徑。
        /// </summary>
        public static string DirImage = DirRoot + "_image" + DirSep;
        /// <summary>
        /// 預設缺圖檔路徑，由 DirImage 初始組成。
        /// </summary>
        public static string NoImagePath = DirImage + "NoImage.jpg";

        /// <summary>
        /// 由 DirRoot 初始組成的草稿目錄路徑。
        /// </summary>
        public static string DirDraft = DirRoot + "_draft" + DirSep;

        /// <summary>
        /// 由 DirRoot 初始組成的暫存目錄路徑。
        /// </summary>
        public static string DirTemp = DirRoot + "_temp" + DirSep;
        #endregion

        #region Db variables
        /// <summary>
        /// 依 DB 種類設定的唯一鍵例外碼清單。
        /// </summary>
        public static int[] UniqueKeyErrorNo = [];

        /// <summary>
        /// 資料庫日期時間格式，由 Init 設定。
        /// </summary>
        public static string DbDtFmt = "";
        /// <summary>
        /// 資料庫日期格式，由 Init 設定。
        /// </summary>
        public static string DbDateFmt = "";

        /// <summary>
        /// 依 DB 種類設定的 SELECT 分頁格式字串。
        /// </summary>
        public static string ReadPageSql = "";

        /// <summary>
        /// 批次刪除 SQL 格式；目前只有 MSSql 初始化分支設定此值。
        /// </summary>
        public static string DeleteRowsSql = "";
        #endregion

        #region others variables
        /// <summary>
        /// 應用須在 Init 前指派的共用組態，不由 Init 自動載入。
        /// </summary>
        public static ConfigDto Config = null!;

        /// <summary>
        /// 由非空 Config.Smtp 解析出的郵件連線設定，可為 null。
        /// </summary>
        public static SmtpDto? Smtp;
        #endregion

        /*
        //constructor
        static _Fun()
        {
        }
        */

        /// <summary>
        /// 先備妥 Config 再設定全域模式、DI 與 DB 格式，必要時原地解密組態；成功回傳空字串，不驗證連線。
        /// </summary>
        public static string Init(bool isDev, IServiceProvider? diBox, DbTypeEnum dbType, 
            AuthTypeEnum authType, bool multiLang = false)
        {
            #region set instance variables
			IsDev = isDev;
            RunModeName = isDev ? "Development" : "Production";
            DiBox = diBox!;
            DbType = dbType;
            //AuthType = (Config.LoginType == LoginTypeEstr.None) ? AuthTypeEnum.None : authType; //無登入必為無權限 !!
            AuthType = authType;
            MultiLang = multiLang;
            //UseDeptRole = useDeptRole;

            Config!.HtmlImageUrl = _Str.AddWebSlash(Config.HtmlImageUrl);
            //Nonce = _Str.NewId();

            //解密敏感組態資料 if need
            if (Config.Encode)
            {
                var key = _Str.ReadFileKey();
                if (string.IsNullOrEmpty(key))
                    return "Decode Failed, Please check Administrator.";

                Config.Db = _Str.DecodeByKey(Config.Db, key).Replace("\\\\","\\");    //config的\到字串會變\\
                Config.Smtp = _Str.DecodeByKey(Config.Smtp, key);
                Config.Redis = _Str.DecodeByKey(Config.Redis, key);
                Config.OtpAuthKey = _Str.DecodeByKey(Config.OtpAuthKey, key);
                Config.SmsAccount = _Str.DecodeByKey(Config.SmsAccount, key);
                Config.SmsPwd = _Str.DecodeByKey(Config.SmsPwd, key);
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
                    UniqueKeyErrorNo = [2601,2627];

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
                    UniqueKeyErrorNo = [1062]; //todo:test

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
                    UniqueKeyErrorNo = [1]; //todo:test

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

        /// <summary>
        /// 產生全部人角色的等值或不等值 SQL 片段；未指定 RoleAll 時回傳 1=1。
        /// </summary>
        public static string RoleAllCond(string fid, bool equal)
        {
            return (RoleAll == "")
                ? "1=1"
                : $"{fid}{(equal ? "=" : "!=")}'{RoleAll}'";
        }

        /// <summary>
        /// 以字串連接根路徑與子目錄，可補尾端分隔符，不建立目錄。
        /// </summary>
        public static string Dir(string folder, bool tailSep = true)
        {
            return DirRoot + folder + (tailSep ? DirSep : "");
        }

        /// <summary>
        /// 透過使用者服務取得目前使用者 Id。
        /// </summary>
        public static string UserId()
        {
            return GetBaseUser().UserId;
        }

        /// <summary>
        /// 透過使用者服務取得目前使用者名稱。
        /// </summary>
        public static string UserName()
        {
            return GetBaseUser().UserName;
        }

        /// <summary>
        /// 透過使用者服務取得是否具有密碼的旗標。
        /// </summary>
        public static bool HasPwd()
        {
            return GetBaseUser().HasPwd;
        }

        /// <summary>
        /// 透過使用者服務取得目前部門 Id。
        /// </summary>
        public static string DeptId()
        {
            return GetBaseUser().DeptId;
        }

        /// <summary>
        /// 判斷授權模式為 Row 且設定要求登入；不驗證目前請求已登入。
        /// </summary>
        public static bool IsAuthRowAndLogin()
        {
            return IsAuthTypeRow() && IsNeedLogin();
        }

        /// <summary>
        /// 判斷目前授權模式是否為 Row。
        /// </summary>
        public static bool IsAuthTypeRow()
        {
            return (AuthType == AuthTypeEnum.Row);
        }

        /// <summary>
        /// 判斷 Config.LoginType 是否不是 None，不代表使用者已驗證。
        /// </summary>
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
        /// 解析 IBaseUserSvc 並讀資料；回傳資料為 null 時補空模型，服務缺失不會自動補註冊。
        /// </summary>
        public static BaseUserDto GetBaseUser()
        {
            var service = (IBaseUserSvc)DiBox!.GetService(typeof(IBaseUserSvc))!;
            return service.GetData() ?? new BaseUserDto();
        }

        /// <summary>
        /// 拋出指定訊息的 Exception，空訊息改用系統錯誤文字。
        /// </summary>
        public static void Except(string error = "")
        {
            throw new Exception(_Str.EmptyToValue(error, SystemError));
        }

        /// <summary>
        /// 將設定的圖片根網址與子路徑直接連接，不進行 URL 編碼。
        /// </summary>
        public static string GetHtmlImageUrl(string subPath)
        {
            return $"{Config!.HtmlImageUrl}{subPath}";
        }

    } //class
    #pragma warning restore CA2211 // 非常數欄位不應可見
}

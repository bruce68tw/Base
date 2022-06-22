using Base.Enums;
using Base.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Base.Services
{
    //global function
    #pragma warning disable CA2211 // 非常數欄位不應可見
    public static class _Fun
    {
        #region constant
        //hidden input for CRSF
        //public const string HideKey = "_hideKey";

        //system session name
        public const string Rows = "_rows";         //rows fid for CrudEdit
        public const string Childs = "_childs";     //childs fid for CrudEdit

        public const string BaseUser = "_BaseUser";         //base user info
        public const string ProgAuthStrs = "_ProgAuthStrs"; //program autu string list

        //c# datetime format, when js send to c#, will match to _fun.MmDtFmt
        public const string CsDtFmt = "yyyy/MM/dd HH:mm:ss";
        public const string CsDtFmt2 = "yyyy/MM/dd HH:mm";

        //carrier
        public const string TextCarrier = "\r\n";     //for string
        public const string HtmlCarrier = "<br>";     //for html

        //crud update/view for AuthType=Data only in xxxEdit.cs
        public const string UserFid = "_userId";
        public const string DeptFid = "_deptId";

        //session timeout(or not login), map to _BR.js
        public const string TimeOutFid = "TimeOut";

        //default pagin rows
        public const int PageRows = 10;

        //indicate error
        public const string PreError = "E:";
        public const string PreBrError = "B:";  //_BR code error
        //public const string PreSystemError = "S:";

        //default view cols for layout(row div, label=2, input=3)(horizontal) 
        public static List<int> DefHoriCols = new() { 2, 3 };

        //directory tail seperator
        public static char DirSep = Path.DirectorySeparatorChar;

        //class name for hide element in RWD phone
        public static string HideRwd = "xg-hide-rwd";
        #endregion

        #region variables which PG can change
        //max export rows count
        public static int MaxExportCount = 3000;

        //crud read for AuthType=Data only in xxxRead.cs
        public static string WhereUserFid = "u.Id='{0}'";
        public static string WhereDeptFid = "u.DeptId='{0}'";

        public static string SystemError = "System Error, Please Contact Administrator.";
        #endregion

        #region input parameters
        //is devironment or not
        public static bool IsDev;

        //private static ServiceContainer _DI;
        public static IServiceProvider DiBox;

        //database type
        public static DbTypeEnum DbType;

        //program auth type
        public static AuthTypeEnum AuthType;
        #endregion

        #region base varibles
        //ap physical path, has right slash
        public static string DirRoot = _Str.GetLeft(AppDomain.CurrentDomain.BaseDirectory, "bin" + DirSep);

        //temp folder
        public static string DirTemp = DirRoot + "_temp" + DirSep;
        #endregion

        #region Db variables
        //datetime format for read/write db
        public static string DbDtFmt;
        public static string DbDateFmt;

        //for read page rows
        public static string ReadPageSql;

        //for delete rows
        public static string DeleteRowsSql;
        #endregion

        #region others variables
        //from config file
        public static ConfigDto Config;

        public static SmtpDto Smtp = default;
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
        /// <param name="dbType"></param>
        /// <param name="authType"></param>
        /// <returns>error msg if any</returns>
        public static string Init(bool isDev, IServiceProvider diBox, DbTypeEnum dbType = DbTypeEnum.MSSql, 
            AuthTypeEnum authType = AuthTypeEnum.None)
        {
            //set instance variables
            IsDev = isDev;
            DiBox = diBox;
            DbType = dbType;
            AuthType = authType;

            Config.HtmlImageUrl = _Str.AddSlash(Config.HtmlImageUrl);

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
            return _Fun.DirRoot + folder + (tailSep ? DirSep : "");
        }

        //get current userId
        public static string UserId()
        {
            return GetBaseUser().UserId;
        }

        public static string DeptId()
        {
            return GetBaseUser().DeptId;
        }

        //check is AuthType=Data
        public static bool IsAuthTypeRow()
        {
            return (AuthType == AuthTypeEnum.Row);
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
            return !_Str.IsEmpty(msg) &&
                msg.Length >= len &&
                msg.Substring(0, len) == PreError;
        }
        */

        /// <summary>
        /// get base user info for base component
        /// </summary>
        /// <returns>BaseUserDto(not null)</returns>
        public static BaseUserDto GetBaseUser()
        {
            var service = (IBaseUserService)DiBox.GetService(typeof(IBaseUserService));
            return service.GetData() ?? new BaseUserDto();
        }

        /// <summary>
        /// check and open db
        /// </summary>
        /// <param name="db"></param>
        /// <param name="hasDb"></param>
        /// <param name="dbStr"></param>
        public static void CheckOpenDb(ref Db db, ref bool hasDb, string dbStr = "")
        {
            hasDb = (db != null);
            if (!hasDb)
                db = new Db(dbStr);
        }

        /// <summary>
        /// check and close db
        /// </summary>
        /// <param name="db"></param>
        /// <param name="hasDb"></param>
        public static async Task CheckCloseDb(Db db, bool hasDb)
        {
            if (!hasDb)
                await db.DisposeAsync();
        }

        public static void Except(string error = "")
        {
            throw new Exception(_Str.EmptyToValue(error, SystemError));
        }

        public static string GetHtmlImageUrl(string subPath)
        {
            return $"{Config.HtmlImageUrl}{subPath}";
        }

    } //class
    #pragma warning restore CA2211 // 非常數欄位不應可見
}

using Base.Enums;
using Base.Models;
using System;
using System.Collections.Generic;

namespace Base.Services
{
    //global function
    public static class _Fun
    {
        #region constant
        //hidden input for CRSF
        //public const string HideKey = "_hideKey";

        //system session name
        public const string BaseUser = "_BaseUser";         //base user info
        public const string ProgAuthStrs = "_ProgAuthStrs"; //program autu string list

        //c# datetime format, when front pass to c#, match to _fun.JsDtFormat
        public const string CsDtFormat = "yyyy/MM/dd HH:mm:ss";
        //moment js datetime format from db
        //public const string JsDtFormat = "yyyy/M/d HH:mm:ss";

        //carrier
        public const string TextCarrier = "\r\n";     //for string
        public const string HtmlCarrier = "<br>";     //for html

        //null string for checking ??
        //public const string NullString = "_null";

        //default view cols for layout(row div, label=2, input=3)(水平) 
        public static List<int> DefHCols = new List<int>() { 2, 3 };
        #endregion

        #region input parameters
        //private static ServiceContainer _DI;
        private static IServiceProvider _di;

        //database type
        private static DbTypeEnum _dbType;

        //program auth type
        private static AuthTypeEnum _authType;

        //dynamic locale or not
        private static bool _dynamicLocale;
        #endregion

        #region base varibles
        //debug mode or not
        public static bool IsDebug = true;

        //ap physical path, has right slash
        public static string DirRoot = _Str.GetLeft(AppDomain.CurrentDomain.BaseDirectory, "bin\\");

        //temp folder
        public static string DirTemp = DirRoot + "_temp\\";
        public static string DirWeb = DirRoot + "wwwroot\\";
        #endregion

        #region Db variables
        //datetime format for read/write db, match to js _date.JsDtFormat
        public static string DbDtFormat;

        //moment js datetime format from db
        //public static string JsDtFormat;

        //for read page rows
        public static string ReadPageSql;

        //for delete rows
        public static string DeleteRowsSql;

        //db status, when db get error, it will change to false
        //public static bool DbStatus = true;
        #endregion

        #region others variables
        //from config file
        public static ConfigDto Config;

        public static SmtpDto Smtp = default(SmtpDto);

        //session type(web only), 1:session, 2:redis, 3:custom(config must set SessionService field, use Activator)
        //public static int SessionServiceType = 1;
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
        public static void Init(IServiceProvider di, DbTypeEnum dbType = DbTypeEnum.MSSql, 
            AuthTypeEnum authType = AuthTypeEnum.None, bool dynamicLocale = false)
        {
            //set instance variables
            _di = di;
            _dbType = dbType;
            _authType = authType;
            _dynamicLocale = dynamicLocale;

            #region set smtp
            var smtp = Config.Smtp;
            if (smtp != "")
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
                    _Log.Error("config Smtp is wrong(7 cols): Host,Port,Ssl,Id,Pwd,FromEmail,FromName");
                }
            }
            #endregion

            #region set DB variables
            //0:select, 1:order by, 2:start row(base 0), 3:rows count
            switch (_dbType)
            {
                case DbTypeEnum.MSSql:
                    DbDtFormat = "yyyy-MM-dd HH:mm:ss";
                    //JsDtFormat = "yyyy/M/d HH:mm:ss";

                    //for sql 2012, more easy
                    //note: offset/fetch not sql argument
                    ReadPageSql = @"
select {0} {1}
offset {2} rows fetch next {3} rows only
";
                    DeleteRowsSql = "delete {0} where {1} in ({2})";    
                    break;

                case DbTypeEnum.MySql:
                    DbDtFormat = "YYYY-MM-DD HH:mm:SS";

                    ReadPageSql = @"
select {0} {1}
limit {2},{3}
";
                    //TODO: set delete sql for MySql
                    //DeleteRowsSql = 
                    break;

                case DbTypeEnum.Oracle:
                    DbDtFormat = "YYYY/MM/DD HH24:MI:SS";                    

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
        }

        //get DI
        public static IServiceProvider GetDI()
        {
            return _di;
        }

        //get current userId
        public static string UserId()
        {
            return GetBaseUser().UserId;
        }

        //get db type
        public static DbTypeEnum GetDbType()
        {
            return _dbType;
        }

        //get auth type
        public static AuthTypeEnum GetAuthType()
        {
            return _authType;
        }

        //get _dynamicLocale
        public static bool IsDynamicLocale()
        {
            return _dynamicLocale;
        }

        //get system error string
        public static ResultDto GetSystemError()
        {
            return new ResultDto() { ErrorMsg = "System Error, Please check admin !" };
        }

        /// <summary>
        /// get base resource for base component
        /// </summary>
        /// <returns>BaseResourceDto</returns>
        public static BaseResDto GetBaseRes()
        {
            var service = (IBaseResService)_di.GetService(typeof(IBaseResService));
            return service.GetData();
        }

        /// <summary>
        /// get base user info for base component
        /// </summary>
        /// <returns>BaseUserInfoDto</returns>
        public static BaseUserDto GetBaseUser()
        {
            var service = (IBaseUserService)_di.GetService(typeof(IBaseUserService));
            return service.GetData();
        }

        /// <summary>
        /// get locale code
        /// </summary>
        /// <returns></returns>
        public static string GetLocale()
        {
            return GetBaseUser().Locale;
        }

        /// <summary>
        /// check and open db
        /// </summary>
        /// <param name="db"></param>
        /// <param name="emptyDb"></param>
        /// <param name="dbStr"></param>
        public static void CheckOpenDb(ref Db db, ref bool emptyDb, string dbStr = "")
        {
            emptyDb = (db == null);
            if (emptyDb)
                db = new Db(dbStr);
        }

        /// <summary>
        /// check and close db
        /// </summary>
        /// <param name="db"></param>
        /// <param name="emptyDb"></param>
        public static void CheckCloseDb(Db db, bool emptyDb)
        {
            if (emptyDb)
                db.Dispose();
        }

    } //class
}

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

        //c# datetime format, when js send to c#, match to _fun.MmDtFmt
        public const string CsDtFmt = "yyyy/MM/dd HH:mm:ss";

        //carrier
        public const string TextCarrier = "\r\n";     //for string
        public const string HtmlCarrier = "<br>";     //for html

        //??
        //crud read for AuthType = Row
        public const string FindUserFid = "u.Id='{0}'";
        public const string FindDeptFid = "u.DeptId='{0}'";
        //crud update/view for AuthType = Row
        public const string UserFid = "_userId";
        public const string DeptFid = "_deptId";

        //default view cols for layout(row div, label=2, input=3)(水平) 
        public static List<int> DefHoriCols = new List<int>() { 2, 3 };
        #endregion

        #region input parameters
        public static bool IsDebug;

        //private static ServiceContainer _DI;
        public static IServiceProvider DiBox;

        //database type
        public static DbTypeEnum DbType;

        //program auth type
        public static AuthTypeEnum AuthType;
        #endregion

        #region base varibles
        //ap physical path, has right slash
        public static string DirRoot = _Str.GetLeft(AppDomain.CurrentDomain.BaseDirectory, "bin\\");

        //temp folder
        public static string DirTemp = DirRoot + "_temp\\";
        #endregion

        #region Db variables
        //datetime format for read/write db
        public static string DbDtFmt;

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
        public static void Init(bool isDebug, IServiceProvider diBox, DbTypeEnum dbType = DbTypeEnum.MSSql, 
            AuthTypeEnum authType = AuthTypeEnum.None)
        {
            //set instance variables
            IsDebug = isDebug;
            DiBox = diBox;
            DbType = dbType;
            AuthType = authType;
            //_dynamicLocale = dynamicLocale;

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
            switch (dbType)
            {
                case DbTypeEnum.MSSql:
                    DbDtFmt = "yyyy-MM-dd HH:mm:ss";

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

                    ReadPageSql = @"
select {0} {1}
limit {2},{3}
";
                    //TODO: set delete sql for MySql
                    //DeleteRowsSql = 
                    break;

                case DbTypeEnum.Oracle:
                    DbDtFmt = "YYYY/MM/DD HH24:MI:SS";                    

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

        /*
        //get DI
        public static IServiceProvider GetDiBox()
        {
            return _diBox;
        }

        public static bool IsDebug()
        {
            return _isDebug;
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
        */

        //get current userId
        public static string UserId()
        {
            return GetBaseUser().UserId;
        }

        public static string DeptId()
        {
            return GetBaseUser().DeptId;
        }

        /*
        //get system error string
        public static ResultDto GetSystemError()
        {
            return new ResultDto() { ErrorMsg = "System Error, Please check admin !" };
        }
        */

        public static bool IsAuthRow()
        {
            return (AuthType == AuthTypeEnum.Row);
        }

        /*
        /// <summary>
        /// get base resource for base component
        /// </summary>
        /// <returns>BaseResourceDto</returns>
        public static BaseResDto GetBaseRes(string locale = "")
        {
            return _Locale.GetBaseRes(locale);
        }
        */

        /// <summary>
        /// get base user info for base component
        /// </summary>
        /// <returns>BaseUserInfoDto</returns>
        public static BaseUserDto GetBaseUser()
        {
            var service = (IBaseUserService)DiBox.GetService(typeof(IBaseUserService));
            return service.GetData();
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

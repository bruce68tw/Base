using Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Base
{
    public class _Login
    {

        //記錄登入成功
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

    }
}

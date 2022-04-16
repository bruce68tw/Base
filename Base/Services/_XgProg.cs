using Base.Enums;
using Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    //program function
    public static class _XgProg
    {
        /// <summary>
        /// check program access right
        /// </summary>
        /// <param name="authStrs">program auth string list, has ',' at begin/end</param>
        /// <param name="prog">program id</param>
        /// <param name="crudEnum">crud function, see CrudFunEstr, empty for controller, value for action</param>
        /// <returns>bool</returns>
        public static bool CheckAuth(string authStrs, string prog, CrudEnum crudEnum)
        {
            if (_Fun.AuthType == AuthTypeEnum.Ctrl)
            {
                //authStr format: ,xxx,xxx,
                return authStrs.Contains("," + prog + ",", StringComparison.CurrentCulture);
            }
            else
            {
                //authStr format: ,xxx:121,xxx:001,
                return (crudEnum == CrudEnum.Empty)
                    ? authStrs.Contains("," + prog + ":")
                    : GetAuthRange(authStrs, prog, crudEnum) != AuthRangeEnum.None;
            }
        }

        /// <summary>
        /// check Create Auth for CRUD Create button
        /// </summary>
        /// <param name="prog"></param>
        /// <returns></returns>
        public static bool CheckCreate(string prog)
        {
            return (_Fun.AuthType != AuthTypeEnum.Action && _Fun.AuthType != AuthTypeEnum.Row)
                ? true
                : CheckAuth(_Fun.GetBaseUser().ProgAuthStrs, prog, CrudEnum.Create);
        }

        /// <summary>
        /// get auth range of crud fun
        /// </summary>
        /// <param name="prog"></param>
        /// <param name="crudEnum"></param>
        /// <param name="authStrs"></param>
        /// <returns></returns>
        public static AuthRangeEnum GetAuthRange(string authStrs, string prog, CrudEnum crudEnum)
        {
            //if (authStrs == null)
            //    authStrs = _Fun.GetBaseUser().ProgAuthStrs;
            //var sep = ",";
            var funList = _Str.GetMid(authStrs, "," + prog + ":", ",");
            var funPos = (int)crudEnum;
            if (funList.Length <= funPos)
                return AuthRangeEnum.None;

            //pos 0, fun is auth row or not(0/1)
            var auth = (AuthRangeEnum)Convert.ToInt32(funList.Substring(funPos, 1));
            return (funList[..1] == "1") ? auth :
                (auth == AuthRangeEnum.None) ? AuthRangeEnum.None : AuthRangeEnum.All;
        }

        /// <summary>
        /// get program auth list
        /// </summary>
        /// <param name="userId">user Id</param>
        /// <returns>has ','(if not empty) at start/end for easy coding</returns>
        public static async Task<string> GetAuthStrsAsync(string userId)
        {
            string sql;
            List<IdStrDto> rows;
            switch (_Fun.AuthType)
            {
                case AuthTypeEnum.Ctrl:
                    //return format: code,...
                    sql = @"
select distinct 
    p.Code as Id
from XpRoleProg rp
join XpUserRole ur on rp.RoleId=ur.RoleId
join XpProg p on rp.ProgId=p.Id
where ur.UserId=@UserId
";
                    rows = await _Db.GetModelsAsync<IdStrDto>(sql, new List<object>() { "UserId", userId });
                    return (rows == null || rows.Count == 0)
                        ? ""
                        : "," + _List.ToStr(rows.Select(a => a.Id).ToList()) + ",";

                case AuthTypeEnum.Action:
                case AuthTypeEnum.Row:
                    //return format: code:xxx,...
                    //concat auth string list, consider different DB type
                    var authStr = _Sql.GetConcat(
                        "cast(max(p.AuthRow) as char(1))",
                        GetFunSql("FunCreate"),
                        GetFunSql("FunRead", true),
                        GetFunSql("FunUpdate", true),
                        GetFunSql("FunDelete", true),
                        GetFunSql("FunPrint", true),
                        GetFunSql("FunExport", true),
                        GetFunSql("FunView"),
                        GetFunSql("FunOther", true)
                    );

                    sql = $@"
select distinct 
    p.Code as Id, {authStr} as Str
from XpRoleProg rp
join XpUserRole ur on rp.RoleId=ur.RoleId
join XpProg p on rp.ProgId=p.Id
where ur.UserId=@UserId
group by p.Code
";
                    rows = await _Db.GetModelsAsync<IdStrDto>(sql, new List<object>() { "UserId", userId });
                    return (rows == null || rows.Count == 0)
                        ? ""
                        : "," + _List.ToStr(rows.Select(a => a.Id + ":" + a.Str).ToList()) + ",";

                //case else
                default:
                    return null;
            }
        }

        private static string GetFunSql(string fid, bool authRow = false)
        {
            return authRow
                ? $"cast((case when max(p.{fid})=0 then 0 when max(p.AuthRow)=1 then max(rp.{fid}) else 1 end) as char(1))"
                : $"cast((case max(p.{fid}) when 1 then max(rp.{fid}) else 0 end) as char(1))";
        }

    } //class
}

using Base.Enums;
using Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// _XgProg -> _Auth
    /// program function, 適用於其他專案, 所以使用 _XgProg, 字首_Xg表示內含商業規則
    /// _Excel.cs >> CrudReadSvc.cs >> this, 所以此檔放在 Base Project !!
    /// </summary>
    public static class _Auth
    {
        /// <summary>
        /// 檢查個人資料權限是否符合目前登入者
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="crudEnum"></param>
        /// <param name="table"></param>
        /// <param name="userFid"></param>
        /// <param name="key"></param>
        /// <param name="userValue"></param>
        /// <param name="db"></param>
        /// <returns>BR error code if any</returns>
        public static async Task<string> CheckAuthUserA(string ctrl, CrudEnum crudEnum, string table, 
            string userFid, string key, Db? db = null)
        {
            var user = _Fun.GetBaseUser();
            var authRange = GetAuthRange(user.ProgAuthStrs, ctrl, crudEnum);
            if (authRange == AuthRangeEnum.None) return _Str.GetBrError(_Fun.FidNoAuthUser);
            if (authRange == AuthRangeEnum.All) return "";

            var sql = $"select {userFid} from {table} where Id=@Id";
            var value = await _Db.GetStrA(sql, new() { "Id", key }, db) ?? "";
            return (value == user.UserId) 
                ? "" : _Str.GetBrError(_Fun.FidNoAuthUser);
        }

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
            if (funList.Length <= funPos) return AuthRangeEnum.None;

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
        public static async Task<string> GetAuthStrsA(string userId, Db? db = null)
        {
            var newDb = _Db.CheckOpenDb(ref db);
            var result = "";
            string sql;
            //List<IdStrDto> rows;
            var authType = _Fun.IsNeedLogin() ? _Fun.AuthType : AuthTypeEnum.None;
            switch (authType)
            {
                case AuthTypeEnum.Ctrl:
                    //return format: code,...
                    sql = @"
select distinct 
    p.Code
from XpRoleProg rp
join XpUserRole ur on rp.RoleId=ur.RoleId
join XpProg p on rp.ProgId=p.Id
where ur.UserId=@UserId
";
                    //rows = await _Db.GetModelsA<IdStrDto>(sql, new(){ "UserId", userId });
                    var list = await db!.GetStrsA(sql, new(){ "UserId", userId });
                    result = (list == null || list.Count == 0)
                        ? "" : ("," + _List.ToStr(list!) + ",");
                    break;

                case AuthTypeEnum.Action:
                case AuthTypeEnum.Row:
                    //return format: code:xxx,...
                    //concat auth string list, consider different DB type
                    var authStr = _Sql.GetConcat(
                        "cast(p.AuthRow as char(1))",
                        GetFunSql("FunCreate"),
                        GetFunSql("FunRead", true),
                        GetFunSql("FunUpdate", true),
                        GetFunSql("FunDelete", true),
                        GetFunSql("FunPrint", true),
                        GetFunSql("FunExport", true),
                        GetFunSql("FunView", true),
                        GetFunSql("FunOther", true)
                    );

                    sql = $@"
select p.Code as Id, {authStr} as Str
from (
    select rp.ProgId,
        FunCreate=max(FunCreate),
        FunRead=max(FunRead),
        FunUpdate=max(FunUpdate),
        FunDelete=max(FunDelete),
        FunPrint=max(FunPrint),
        FunExport=max(FunExport),
        FunView=max(FunView),
        FunOther=max(FunOther)
    from XpRoleProg rp
    join XpUserRole ur on rp.RoleId=ur.RoleId
    where ur.UserId='{userId}'
    group by rp.ProgId
) a 
join XpProg p on a.ProgId=p.Id
";
                    /*
                    sql = $@"
select distinct 
    p.Code as Id, {authStr} as Str
from XpRoleProg rp
join XpUserRole ur on rp.RoleId=ur.RoleId
join XpProg p on rp.ProgId=p.Id
where ur.UserId=@UserId
group by p.Code
";*/
                    var rows = await _Db.GetModelsA<IdStrDto>(sql);
                    result = (rows == null || rows.Count == 0)
                        ? ""
                        : "," + _List.ToStr(rows.Select(a => a.Id + ":" + a.Str).ToList()) + ",";
                    break;

                //case else
                default:
                    result = "";
                    break;
            }

            await _Db.CheckCloseDbA(db!, newDb);
            return result;
        }

        private static string GetFunSql(string fid, bool authRow = false)
        {
            return authRow
                ? $"cast((case when p.{fid}=0 then 0 when p.AuthRow=1 then a.{fid} when a.{fid} > 0 then 1 else 0 end) as char(1))"
                : $"cast((case when p.{fid}=0 then 0 when a.{fid} > 0 then 1 else 0 end) as char(1))";
        }


        /// <summary>
        /// get auth prog id list
        /// </summary>
        /// <returns>return [] if null</returns>
        public static List<string> GetAuthProgs()
        {
            //get authStrs
            var progs = new List<string>();
            var user = _Fun.GetBaseUser();
            var authStrs = user.ProgAuthStrs;
            if (_Str.IsEmpty(authStrs))
                return progs;

            //remove ',' at start/end
            authStrs = authStrs[1..^1];

            //get prog string list
            switch (_Fun.AuthType)
            {
                case AuthTypeEnum.Ctrl:
                    //do nothing
                    progs = _Str.ToList(authStrs);
                    break;

                case AuthTypeEnum.Action:
                case AuthTypeEnum.Row:
                    var list = authStrs.Split(',');
                    foreach (var item in list)
                        progs.Add(_Str.GetLeft(item, ":"));
                    break;
            }
            return progs;
        }

        /// <summary>
        /// get prog menu from session, called by _Layout.cshtml for show menu
        /// </summary>
        /// <returns>return [] if null</returns>
        public static async Task<List<MenuDto>> GetMenuA()
        {
            /*
            //get authStrs
            var data = new List<MenuDto>();
            var baseUser = _Fun.GetBaseUser();
            var authStrs = baseUser.ProgAuthStrs;
            if (_Str.IsEmpty(authStrs))
                return data;

            //remove ',' at start/end
            authStrs = authStrs[1..^1];
            */

            //get prog string list
            var progs = GetAuthProgs();
            if (progs.Count == 0)
                return [];

            //get Program name from XpProg
            var sql = $@"
select Code, Name, Url, Sort
from dbo.XpProg
where Code in ({_List.ToStr(progs!, true)})
order by Sort
";
            return await _Db.GetModelsA<MenuDto>(sql) ?? [];
        }

        /// <summary>
        /// 過濾 menu
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="authRow"></param>
        /// <returns></returns>
        public static void FilterMenu(List<MenuDto> menus)
        {
            //移除第2層(無權限功能)
            var progs = GetAuthProgs();
            foreach (var menu in menus)
                menu.Items.RemoveAll(a => !progs.Contains(a.Code));

            //移除第1層功能(Code有值表示沒有Items)
            menus.RemoveAll(a => !string.IsNullOrEmpty(a.Code) && !progs.Contains(a.Code));

            //移除第1層功能空的子功能上層
            menus.RemoveAll(a => string.IsNullOrEmpty(a.Code) && a.Items.Count == 0);
        }

    } //class
}

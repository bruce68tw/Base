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
        /// 檢查來源IP是否在組態檔允許的清單內
        /// </summary>
        /// <param name="clientIp"></param>
        /// <returns></returns>
        public static bool CheckClientIp(string clientIp)
        {
            return (_Fun.Config.ClientIps.IndexOf(clientIp) >= 0);
        }

        /// <summary>
        /// check program access right
        /// </summary>
        /// <param name="authStr">program auth string list, has ',' at begin/end</param>
        /// <param name="prog">program id</param>
        /// <param name="crudEnum">crud function, see CrudFunEstr, empty for controller, value for action</param>
        /// <returns>bool</returns>
        public static bool CheckAuth(string prog, CrudEnum crudEnum, string authStr = "")
        {
            if (string.IsNullOrEmpty(authStr))
                authStr = _Fun.GetBaseUser().ProgAuthStrs;

            if (_Fun.AuthType == AuthTypeEnum.Ctrl)
            {
                //authStr format: ,xxx,xxx,
                return authStr.Contains("," + prog + ",", StringComparison.CurrentCulture);
            }
            else
            {
                //authStr format: ,xxx:121,xxx:001,
                return (crudEnum == CrudEnum.Empty)
                    ? authStr.Contains("," + prog + ":")
                    : GetAuthRange(prog, crudEnum, authStr) != AuthRangeEnum.None;
            }
        }

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
            var br = _Fun.GetBaseUser();
            var authRange = GetAuthRange(ctrl, crudEnum, br.ProgAuthStrs);
            if (authRange == AuthRangeEnum.None) return _Str.GetBrError(_Fun.FidNoAuthUser);
            if (authRange == AuthRangeEnum.All) return "";

            var sql = $"select {userFid} from {table} where Id=@Id";
            var value = await _Db.GetStrA(sql, ["Id", key], db) ?? "";
            return (value == br.UserId) 
                ? "" : _Str.GetBrError(_Fun.FidNoAuthUser);
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
                : CheckAuth(prog, CrudEnum.Create);
        }

        /// <summary>
        /// get auth range of one crud fun
        /// </summary>
        /// <param name="prog"></param>
        /// <param name="fun"></param>
        /// <param name="authStr"></param>
        /// <returns></returns>
        public static AuthRangeEnum GetAuthRange(string prog, CrudEnum fun, string authStr = "")
        {
            if (string.IsNullOrEmpty(authStr))
                authStr = _Fun.GetBaseUser().ProgAuthStrs;

            //var sep = ",";
            var funList = _Str.GetMid(authStr, "," + prog + ":", ",");
            var funPos = (int)fun;
            if (funList.Length <= funPos) return AuthRangeEnum.None;

            //pos 0, fun is auth row or not(0/1)
            var auth = (AuthRangeEnum)Convert.ToInt32(funList.Substring(funPos, 1));
            return (funList[..1] == "1") ? auth :
                (auth == AuthRangeEnum.None) ? AuthRangeEnum.None : AuthRangeEnum.All;
        }

        /// <summary>
        /// GetAuthStrsA -> GetAuthStrA
        /// get program auth list
        /// </summary>
        /// <param name="roleAll">所有人都具備的角色</param>
        /// <param name="userId">user Id</param>
        /// <returns>has ','(if not empty) at start/end for easy coding</returns>
        public static async Task<string> GetAuthStrA(string userId, Db? db = null)
        {
            var newDb = _Db.CheckOpenDb(ref db);
            var result = "";
            string sql, sqlTableWhere;
            List<object> args = ["RoleId", _Fun.RoleAll, "DeptId", _Fun.DeptAll];   //"所有人" 角色
            var authType = _Fun.IsNeedLogin() ? _Fun.AuthType : AuthTypeEnum.None;
            switch (authType)
            {
                case AuthTypeEnum.Ctrl:
                    sqlTableWhere = _Fun.UseDeptRole
                        ? $@"
from dbo.XpRoleProg rp
join dbo.XpProg p on rp.ProgId=p.Id
join dbo.XpDeptRole dr on rp.DeptRoleId=dr.Id
join dbo.XpUserRole ur on dr.RoleId=ur.RoleId or (@RoleId != '' and dr.RoleId=@RoleId)
join dbo.XpUser u on ur.UserId=u.Id and (dr.DeptId=u.DeptId or (@DeptId != '' and dr.DeptId=@DeptId))
where ur.UserId='{userId}'
"
                        : $@"
from dbo.XpRoleProg rp
join dbo.XpProg p on rp.ProgId=p.Id
join dbo.XpUserRole ur on rp.SourceId=ur.RoleId or (@RoleId != '' and rp.SourceId=@RoleId)
where ur.UserId='{userId}'
";

                    //return format: code,...
                    sql = $@"
select distinct 
    p.Code
{sqlTableWhere}
";
                    //rows = await _Db.GetModelsA<IdStrDto>(sql, new(){ "UserId", userId });
                    var list = await db!.GetStrsA(sql, args);
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

                    sqlTableWhere = _Fun.UseDeptRole
                        ? $@"
from dbo.XpRoleProg rp
join dbo.XpDeptRole dr on rp.DeptRoleId=dr.Id
join dbo.XpUserRole ur on dr.RoleId=ur.RoleId or (@RoleId != '' and dr.RoleId=@RoleId)
join dbo.XpUser u on ur.UserId=u.Id and (dr.DeptId=u.DeptId or (@DeptId != '' and dr.DeptId=@DeptId))
where ur.UserId='{userId}'
"
                        : $@"
from dbo.XpRoleProg rp
join dbo.XpUserRole ur on rp.SourceId=ur.RoleId or (@RoleId != '' and rp.SourceId=@RoleId)
where ur.UserId='{userId}' 
";

                    sql = $@"
select p.Code as Id, {authStr} as Str
from (
    select rp.ProgId,
        FunCreate=max(rp.FunCreate),
        FunRead=max(rp.FunRead),
        FunUpdate=max(rp.FunUpdate),
        FunDelete=max(rp.FunDelete),
        FunPrint=max(rp.FunPrint),
        FunExport=max(rp.FunExport),
        FunView=max(rp.FunView),
        FunOther=max(rp.FunOther)
    {sqlTableWhere}
    group by rp.ProgId
) a 
join XpProg p on a.ProgId=p.Id
";
                    var rows = await _Db.GetModelsA<IdStrDto>(sql, args);
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
        /// get auth prog id(only) list
        /// </summary>
        /// <returns>return [] if null</returns>
        private static List<string> GetAuthProgs(BaseUserDto user)
        {
            //get authStrs
            var progs = new List<string>();
            //var user = _Fun.GetBaseUser();
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
        /// GetMenuA -> GetMenu1A (表示1階)
        /// get 1階menu from session, called by _Layout.cshtml for show menu
        /// </summary>
        /// <param name="superMode"></param>
        /// <param name="fnSetPwd">無密碼時開啟SetPwd</param>
        /// <returns>return [] if null</returns>
        public static async Task<List<MenuDto>> GetMenu1A(string fnSetPwd, bool superMode = false)
        {
            //get Program name from XpProg
            var sql = $@"
select Code, Name, Url, Sort
from dbo.XpProg
order by Sort
";
            var menus = await _Db.GetModelsA<MenuDto>(sql);
            if (!superMode)
                RemoveEmptyMenu(menus, fnSetPwd, false);

            return menus ?? [];
        }

        /// <summary>
        /// 加上menu群組
        /// </summary>
        /// <returns></returns>
        public static async Task<List<MenuDto>> GetMenu2A(string fnSetPwd, bool superMode = false)
        {
            var sql = @"
select 
    p.Code, p.Sort,
	p.Name, p.Url,
	GroupName=c.Name
from dbo.XpProg p
join dbo.XpCode c on c.Type='MenuGroup' and p.MenuGroup=c.Value
where p.Status=1
order by c.Sort, p.Sort
";
            var allMenus = await _Db.GetModelsA<MenuDto>(sql);
            if (allMenus == null)
                return [];

            //分群組(2層)
            var menus = allMenus!.GroupBy(a => a.GroupName)
                .Select(a => new MenuDto
                {
                    Name = a.Key,
                    Items = a.Select(b => new MenuDto
                    {
                        Code = b.Code,
                        Name = b.Name,
                        Url = b.Url,
                        Sort = b.Sort,
                    }).ToList()
                })
                .ToList();

            if (!superMode)
                RemoveEmptyMenu(menus, fnSetPwd, true);

            return menus;
        }

        /// <summary>
        /// 過濾掉無權限的 menu, 同時移除空白的 items
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="authRow"></param>
        /// <returns></returns>
        private static void RemoveEmptyMenu(List<MenuDto>? menus, string fnSetPwd, bool has2Level)
        {
            if (menus == null) return;

            //如果無密碼則只保留setPwd功能
            var user = _Fun.GetBaseUser();
            var progs = user.HasPwd ? GetAuthProgs(user) :
                string.IsNullOrEmpty(fnSetPwd) ? [] :
                [fnSetPwd];

            //移除第1層無權限功能(Code有值表示沒有Items)
            menus.RemoveAll(a => !string.IsNullOrEmpty(a.Code) && !progs.Contains(a.Code));

            //移除第2層無權限功能
            if (has2Level)
            {
                foreach (var menu in menus)
                    menu.Items.RemoveAll(a => !progs.Contains(a.Code));

                //移除第1層空白功能Items
                menus.RemoveAll(a => string.IsNullOrEmpty(a.Code) && a.Items.Count == 0);
            }
        }

    } //class
}

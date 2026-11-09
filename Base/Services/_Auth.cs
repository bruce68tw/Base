using Base.Enums;
using Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// 作業權限字串、個人範圍與功能表組合；依賴目前使用者、設定與 Xp 系列表格。
    /// </summary>
    public static class _Auth
    {
        /// <summary>
        /// 以完全相等比對允許的 IP 清單，不解析 DNS 或 CIDR。
        /// </summary>
        public static bool CheckClientIp(string clientIp)
        {
            return (_Fun.Config.ClientIps.IndexOf(clientIp) >= 0);
        }

        /// <summary>
        /// 依授權模式檢查作業或子功能；未傳權限字串時取目前使用者，並非所有模式自動放行。
        /// </summary>
        public static bool CheckAuth(string prog, CrudEnum fun, string authStr = "")
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
                return (fun == CrudEnum.Empty)
                    ? authStr.Contains("," + prog + ":")
                    : GetAuthRange(prog, fun, authStr) != AuthRangeEnum.None;
            }
        }

        /// <summary>
        /// 依 Id 查使用者欄位並比對目前使用者；All 通過，None 失敗，其餘一律按個人範圍檢查。
        /// </summary>
        public static async Task<string> CheckAuthUserA(string ctrl, CrudEnum fun, string table, 
            string userFid, string key, Db? db = null)
        {
            var br = _Fun.GetBaseUser();
            var authRange = GetAuthRange(ctrl, fun, br.ProgAuthStrs);
            if (authRange == AuthRangeEnum.None) return _Str.GetBrError(_Fun.FidNoAuthUser);
            if (authRange == AuthRangeEnum.All) return "";

            var sql = $"select {userFid} from {table} where Id=@Id";
            var value = await _Db.GetStrA(sql, ["Id", key], db) ?? "";
            return (value == br.UserId) 
                ? "" : _Str.GetBrError(_Fun.FidNoAuthUser);
        }

        /// <summary>
        /// Action 或 Row 模式檢查新增功能，其餘模式直接回傳 true。
        /// </summary>
        public static bool CheckCreate(string prog)
        {
            return (_Fun.AuthType != AuthTypeEnum.Action && _Fun.AuthType != AuthTypeEnum.Row)
                ? true
                : CheckAuth(prog, CrudEnum.Create);
        }

        /// <summary>
        /// 依 CrudEnum 位置解析單一權限數字；位置缺失為 None，非資料列模式的非零值轉成 All。
        /// </summary>
        public static AuthRangeEnum GetAuthRange(string prog, CrudEnum fun, string authStr = "")
        {
            if (string.IsNullOrEmpty(authStr))
                authStr = _Fun.GetBaseUser().ProgAuthStrs;

            //簽核需要"新增"權限
            //if (fun == CrudEnum.Sign)
            //    fun = CrudEnum.Create;

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
        /// 依使用者角色查詢權限並組成逗號包圍字串；只釋放自行建立的 Db。
        /// </summary>
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
join dbo.XpUserRole ur on (ur.UserId='{userId}' and dr.RoleId=ur.RoleId) or (@RoleId != '' and dr.RoleId=@RoleId)
join dbo.XpUser u on ur.UserId=u.Id and (dr.DeptId=u.DeptId or (@DeptId != '' and dr.DeptId=@DeptId))
"
                        : $@"
from dbo.XpRoleProg rp
join dbo.XpProg p on rp.ProgId=p.Id
join dbo.XpUserRole ur on (ur.UserId='{userId}' and rp.RoleId=ur.RoleId) or (@RoleId != '' and rp.RoleId=@RoleId)
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
join dbo.XpUserRole ur on (ur.UserId='{userId}' and dr.RoleId=ur.RoleId) or (@RoleId != '' and dr.RoleId=@RoleId)
join dbo.XpUser u on ur.UserId=u.Id and (dr.DeptId=u.DeptId or (@DeptId != '' and dr.DeptId=@DeptId))
"
                        : $@"
from dbo.XpRoleProg rp
join dbo.XpUserRole ur on (ur.UserId='{userId}' and rp.RoleId=ur.RoleId) or (@RoleId != '' and rp.RoleId=@RoleId)
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
        /// get auth prog code(only) list
        /// </summary>
        /// <returns>return [] if null</returns>
        private static List<string> GetAuthProgCodes(BaseUserDto user)
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

        public static async Task<List<string>> GetAuthProgNamesA(BaseUserDto user)
        {
            var codes = GetAuthProgCodes(user);
            var sql = $@"
select Name
from dbo.XpProg
where Code in ({_List.ToStr(codes, true)})
order by Sort
";
            return await _Db.GetStrsA(sql) ?? [];
        }

        /// <summary>
        /// 讀取單層功能表；非 superMode 且 fnSetPwd 非空時才依使用者權限過濾。
        /// </summary>
        public static async Task<List<MenuDto>> GetMenu1A(string fnSetPwd = "", bool superMode = false)
        {
            //get Program name from XpProg
            var sql = $@"
select Code, Name, Url, Sort
from dbo.XpProg
order by Sort
";
            var menus = await _Db.GetModelsA<MenuDto>(sql);
            if (!superMode && !string.IsNullOrEmpty(fnSetPwd))
                RemoveEmptyMenu(menus, fnSetPwd, false);

            return menus ?? [];
        }

        /// <summary>
        /// 依 MenuGroup 組合啟用中的兩層選單；非 superMode 時原地過濾無權限項目。
        /// </summary>
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
        /// 原地移除無權限項目與空群組；無密碼使用者依 fnSetPwd 限制可見作業。
        /// </summary>
        public static void RemoveEmptyMenu(List<MenuDto>? menus, string fnSetPwd, bool has2Level)
        {
            if (menus == null) return;

            //如果無密碼則只保留setPwd功能
            var user = _Fun.GetBaseUser();
            var progs = user.HasPwd ? GetAuthProgCodes(user) :
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

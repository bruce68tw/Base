using Base.Enums;
using Base.Models;
using System.Collections.Generic;
using System.Linq;

namespace Base.Services
{
    //program function
    public static class _Prog
    {
        /// <summary>
        /// check program access right
        /// </summary>
        /// <param name="progList">program list</param>
        /// <param name="prog">program id</param>
        /// <param name="crudFun">crud function, see CrudFunEstr, empty for controller, value for action</param>
        /// <returns>bool</returns>
        public static bool CheckAuth(string progList, string prog, CrudFunEnum crudFun)
        {
            var comma = ",";
            progList = comma + progList + comma;
            if (crudFun == CrudFunEnum.Empty)
            {
                //prog add tail of ','
                return progList.Contains(comma + prog + comma);
            }
            else
            {
                //prog add tail of ':'
                var funList = _Str.GetMid(progList, comma + prog + ":", comma);
                var funPos = (int)crudFun;
                return (funList.Length > funPos && funList.Substring(funPos, 1) == "1");
            }
        }

        /// <summary>
        /// get program auth list
        /// </summary>
        /// <param name="userId">user Id</param>
        /// <returns></returns>
        public static List<ProgAuthDto> GetAuthList(string userId)
        {
            string sql;
            switch (_Fun.GetAuthType())
            {
                case AuthTypeEnum.Ctrl:
                    sql = @"
select distinct 
    p.Code as ProgCode, p.Name as ProgName
from RoleProg rp
join UserRole ur on rp.RoleId=ur.RoleId
join Prog p on rp.ProgId = p.Id
where ur.UserId=@UserId
";
                    return _Db.GetModels<ProgAuthDto>(sql, new List<object>() { "UserId", userId });

                case AuthTypeEnum.Action:
                    //concat auth string list, consider different DB type
                    var authList = _Sql.GetConcat(
                            "cast((case max(p.FunCreate) when 1 then max(rp.FunCreate) else 0 end) as char(1))",
                            "cast((case max(p.FunRead) when 1 then max(rp.FunRead) else 0 end) as char(1))",
                            "cast((case max(p.FunUpdate) when 1 then max(rp.FunUpdate) else 0 end) as char(1))",
                            "cast((case max(p.FunDelete) when 1 then max(rp.FunDelete) else 0 end) as char(1))",
                            "cast((case max(p.FunPrint) when 1 then max(rp.FunPrint) else 0 end) as char(1))",
                            "cast((case max(p.FunExport) when 1 then max(rp.FunExport) else 0 end) as char(1))",
                            "cast((case max(p.FunView) when 1 then max(rp.FunView) else 0 end) as char(1))",
                            "cast((case max(p.FunOther) when 1 then max(rp.FunOther) else 0 end) as char(1))"
                    );
                    sql = $@"
select distinct 
    p.Code as ProgCode, p.Name as ProgName, {authList} as AuthList
from RoleProg rp
join UserRole ur on rp.RoleId=ur.RoleId
join Prog p on rp.ProgId = p.Id
where ur.UserId=@UserId
";
                    return _Db.GetModels<ProgAuthDto>(sql, new List<object>() { "UserId", userId });

                //TODO: other case
                //case AuthTypeEnum.Row:
                //    return null;

                //case else
                default:
                    return null;
            }
        }

        /// <summary>
        /// get auth string list for check auth
        /// </summary>
        /// <param name="authList"></param>
        /// <returns></returns>
        public static string GetAuthStrs(List<ProgAuthDto> authList)
        {
            if (authList == null || authList.Count == 0)
                return "";

            switch (_Fun.GetAuthType())
            {
                case AuthTypeEnum.Ctrl:
                    return _List.ToStr(authList.Select(a => a.ProgCode).ToList(), false);

                case AuthTypeEnum.Action:
                    return _List.ToStr(authList.Select(a => a.ProgCode + ":" + a.AuthStr).ToList(), false);

                default:
                    return "";
            }
        }

    } //class
}

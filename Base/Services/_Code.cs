using Base.Enums;
using Base.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// 處理 XpCode table
    /// </summary>
    public class _Code
    {
        public const string AuthRange = "xAuthRange";
        public const string SignStatus = "xfSignStatus";
        public const string NodeType = "xfNodeType";
        public const string SignerType = "xfSignerType";
        public const string AndOr = "xfAndOr";
        public const string LineOp = "xfLineOp";
        public const string LineFromType = "xfLineFromType";
        public const string FlowStatus = "xfFlowStatus";
        //public const string SignStatus = "xfSignStatus";

        public static async Task<List<IdStrDto>> SqlToCodesA(string sql, Db? db = null, List<object>? args = null)
        {
            //return await _Db.SqlToCodesA(sql, null, db) ?? [];
            return await _Db.GetModelsA<IdStrDto>(sql, args, db) ?? [];
        }

        public static async Task<List<IdStrExtDto>> SqlToCodeExtsA(string sql, Db? db = null, List<object>? args = null)
        {
            return await _Db.GetModelsA<IdStrExtDto>(sql, args, db) ?? [];
        }

        public static async Task<List<IdStrExt2Dto>> SqlToCodeExt2sA(string sql, Db? db = null, List<object>? args = null)
        {
            return await _Db.GetModelsA<IdStrExt2Dto>(sql, args, db) ?? [];
        }

        //get code table rows
        public static async Task<List<IdStrDto>> TypeToCodesA(string type, Db? db = null, string locale = "")
        {
            var name = string.IsNullOrEmpty(locale) ? "Name" : "Name_" + locale;
            var sql = $@"
select Value as Id, {name} as Str
from dbo.XpCode
where Type='{type}'
order by Sort";
            return await SqlToCodesA(sql, db);
        }

        public static async Task<List<IdStrDto>> TableToCodesA(string table, Db? db = null, string? sort = null)
        {
            //return await _Db.TableToCodesA("XpFlow", db, sort) ?? [];
            sort ??= "Id";
            var sql = $@"
select Id, Name as Str
from {table}
order by {sort}";
            return await SqlToCodesA(sql, db);
        }

        public static async Task<List<IdStrExtDto>> TableToCodeExtsA(string table, string extFid, 
            Db? db = null, string? sort = null)
        {
            sort ??= "Id";
            var sql = @$"
select Id, Name as Str, {extFid} as Ext
from {table}
order by {sort}";
            return await SqlToCodeExtsA(sql, db);
        }

        //get code table rows
        public static async Task<List<IdStrExtDto>> TypesToCodesA(string[] types, Db? db = null, string locale = "")
        {
            var name = string.IsNullOrEmpty(locale) ? "Name" : "Name_" + locale;
            var sql = $@"
select Value as Id, {name} as Str, Type as Ext
from dbo.XpCode
where Type in ({_Array.ToStr(types, true)})
order by Type, Sort";
            return await SqlToCodeExtsA(sql, db);
        }

        public static List<IdStrDto> AddEmpty(List<IdStrDto>? codes, string plsSelect)
        {
            codes ??= [];
            codes.Insert(0, new IdStrDto()
            {
                Id = "",
                Str = plsSelect,
            });
            return codes;
        }

        public static async Task<List<IdStrDto>> RolesA(Db? db = null)
        {
            var sql = $@"
select 
    Id, [Name] as Str
from dbo.XpRole
where {_Fun.RoleAllCond("Id", false)}
order by Sort
";
            return await SqlToCodesA(sql, db);
        }

        public static async Task<List<IdStrDto>> AuthRangeA(Db? db = null)
        {
            return await TypeToCodesA(AuthRange, db);
        }

        public static async Task<List<IdStrDto>> XpFlowA(Db? db = null)
        {
            return await TableToCodesA("XpFlow", db);
        }

        //讀取某個部門的角色，給使用者編輯用
        public static async Task<List<IdStrDto>> DeptRoleA(Db? db = null)
        {
            var sql = $@"
select dr.Id, Str=d.Name+'-'+r.Name
from dbo.XpDeptRole dr
join dbo.XpDept d on dr.DeptId=d.Id
join dbo.XpRole r on dr.RoleId=r.Id
order by d.Sort, r.Sort
";
            return await SqlToCodesA(sql, db);
        }

        //1階
        public static async Task<List<IdStrDto>> Prog1A(Db? db = null)
        {
            return await TableToCodesA("dbo.XpProg", db, "Sort");
        }

        //2階, 排序：先依MenuGroup的Sort，再依Prog的Sort
        public static async Task<List<IdStrDto>> Prog2A(Db? db = null)
        {
            var sql = $@"
select p.Id, p.Name as Str
from dbo.XpProg p
join dbo.XpCode c on c.Type='MenuGroup' and p.MenuGroup=c.Value
order by c.Sort, p.Sort";
            return await SqlToCodesA(sql, db);
        }

        public static async Task<List<IdStrDto>> DeptA(Db? db = null)
        {
            return await TableToCodesA("dbo.XpDept", db, "Sort");
        }

        /// <summary>
        /// XpCode list 轉換成多組簽核資料下拉式欄位清單
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static List<IdStrDto>? CodesToSignStatuses(List<IdStrExtDto>? rows)
        {
            return FilterByExt(rows, SignStatus)!
                .Where(a => a.Id is SignStatusEstr.Agree or SignStatusEstr.Back)
                .ToList();
        }

        public static async Task<List<IdStrDto>> SignStatusA(bool forSign, Db? db = null)
        {
            //return 
            if (forSign)
            {
                var sql = $@"
select Id=Value, Str=Name
from dbo.XpCode
where Type='{SignStatus}'
and Ext=1
order by Sort
";
                return await SqlToCodesA(sql, db);
            }
            else
            {
                return await TypeToCodesA(SignStatus, db);
            }
        }

        /*
        //FilterArray -> FilterJsons
        //filter json array
        public static JArray? FilterJsons(JArray rows, string fid, string value)
        {
            //if (rows == null) return null;

            var finds = new JArray();
            foreach (var row in rows)
                if (row[fid]!.ToString() == value) finds.Add(row);
            return (finds.Count == 0)
                ? null : finds;
        }
        */

        //FilterRows -> FilterList -> FilterByExt
        public static List<IdStrDto>? FilterByExt(List<IdStrExtDto>? rows, string value)
        {
            return (rows == null || rows.Count == 0)
                ? null
                : rows.Where(a => a.Ext == value)
                    .Select(a => new IdStrDto { Id = a.Id, Str = a.Str })
                    .ToList();
        }

        /// <summary>
    }//class
}

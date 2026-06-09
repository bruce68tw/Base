using Base.Enums;
using Base.Models;
using Newtonsoft.Json.Linq;
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

        public static async Task<List<IdStrDto>?> AuthRangeA(Db? db = null)
        {
            return await _List.TypeToListA(AuthRange, db);
        }

        public static async Task<List<IdStrDto>?> XpFlowA(Db? db = null)
        {
            return await _Db.TableToCodesA("XpFlow", db);
        }

        //讀取某個部門的角色，給使用者編輯用
        public static async Task<List<IdStrDto>?> DeptRoleA(Db? db = null)
        {
            var sql = $@"
select dr.Id, Str=d.Name+'-'+r.Name
from dbo.XpDeptRole dr
join dbo.XpDept d on dr.DeptId=d.Id
join dbo.XpRole r on dr.RoleId=r.Id
order by d.Sort, r.Sort
";
            return await _Db.SqlToCodesA(sql, null, db);
        }

        //1階
        public static async Task<List<IdStrDto>?> Prog1A(Db? db = null)
        {
            var sql = $@"
select p.Id, p.Name as Str
from dbo.XpProg p
order by p.Sort";
            return await _Db.SqlToCodesA(sql, null, db);
        }

        //2階, 排序：先依MenuGroup的Sort，再依Prog的Sort
        public static async Task<List<IdStrDto>?> Prog2A(Db? db = null)
        {
            var sql = $@"
select p.Id, p.Name as Str
from dbo.XpProg p
join dbo.XpCode c on c.Type='MenuGroup' and p.MenuGroup=c.Value
order by c.Sort, p.Sort";
            return await _Db.SqlToCodesA(sql, null, db);
        }

        public static async Task<List<IdStrDto>?> DeptA(Db? db = null)
        {
            return await _Db.TableToCodesA("XpDept", db, "Sort");
        }

        /// <summary>
        /// XpCode list 轉換成多組簽核資料下拉式欄位清單
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static List<IdStrDto>? CodesToSignStatuses(List<IdStrExtDto>? rows)
        {
            return _Code.FilterList(rows, SignStatus)!
                .Where(a => a.Id is SignStatusEstr.Agree or SignStatusEstr.Back)
                .ToList();
        }

        public static async Task<List<IdStrDto>?> SignStatusA(bool forSign, Db? db = null)
        {
            //return 
            if (forSign)
            {
                var sql = $@"
select Id=Value, Str=Name
from dbo.XpCode
where Type='{_Code.SignStatus}'
and Ext=1
order by Sort
";
                return await _Db.SqlToCodesA(sql, null, db);

            }
            else
            {
                return await _List.TypeToListA(_Code.SignStatus, db);
            }
        }

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

        //FilterRows -> FilterList
        public static List<IdStrDto>? FilterList(List<IdStrExtDto>? rows, string value)
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

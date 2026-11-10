using Base.Enums;
using Base.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// 查詢 Xp 代碼與選項清單；SQL 結構和部分條件直接組合，設定須可信，空查詢結果多轉為空清單。
    /// </summary>
    public class _Code
    {
        /// <summary>
        /// 資料存取範圍的代碼分類。
        /// </summary>
        public const string AuthRange = "xAuthRange";
        /// <summary>
        /// 個別簽核狀態的代碼分類。
        /// </summary>
        public const string SignStatus = "xfSignStatus";
        /// <summary>
        /// 流程節點種類的代碼分類。
        /// </summary>
        public const string NodeType = "xfNodeType";
        /// <summary>
        /// 簽核對象種類的代碼分類。
        /// </summary>
        public const string SignerType = "xfSignerType";
        /// <summary>
        /// 條件組合方式的代碼分類。
        /// </summary>
        public const string AndOr = "xfAndOr";
        /// <summary>
        /// 流程連線運算子的代碼分類。
        /// </summary>
        public const string LineOp = "xfLineOp";
        /// <summary>
        /// 連線來源種類的代碼分類。
        /// </summary>
        public const string LineFromType = "xfLineFromType";
        /// <summary>
        /// 流程整體狀態的代碼分類。
        /// </summary>
        public const string FlowStatus = "xfFlowStatus";
        //public const string SignStatus = "xfSignStatus";

        /// <summary>
        /// 查詢 Id、Str 選項；無資料或底層查詢失敗回傳空清單。
        /// </summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="db">要沿用的資料庫物件</param>
        /// <param name="args">依名稱和值交錯排列的 SQL 參數清單。</param>
        public static async Task<List<IdStrDto>> SqlToCodesA(string sql, Db? db = null, List<object>? args = null)
        {
            //return await _Db.SqlToCodesA(sql, null, db) ?? [];
            return await _Db.GetModelsA<IdStrDto>(sql, args, db) ?? [];
        }

        /// <summary>
        /// 查詢附 Ext 的選項，無結果回傳空清單。
        /// </summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="db">要沿用的資料庫物件</param>
        /// <param name="args">依名稱和值交錯排列的 SQL 參數清單。</param>
        public static async Task<List<IdStrExtDto>> SqlToCodeExtsA(string sql, Db? db = null, List<object>? args = null)
        {
            return await _Db.GetModelsA<IdStrExtDto>(sql, args, db) ?? [];
        }

        /// <summary>
        /// 查詢附 Ext、Ext2 的選項，無結果回傳空清單。
        /// </summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="db">要沿用的資料庫物件</param>
        /// <param name="args">依名稱和值交錯排列的 SQL 參數清單。</param>
        public static async Task<List<IdStrExt2Dto>> SqlToCodeExt2sA(string sql, Db? db = null, List<object>? args = null)
        {
            return await _Db.GetModelsA<IdStrExt2Dto>(sql, args, db) ?? [];
        }

        /// <summary>
        /// 依分類查 XpCode；locale 非空時選取 Name_語系 欄位，依 Sort 排序。
        /// </summary>
        /// <param name="type">要套用的分類或格式值</param>
        /// <param name="db">要沿用的資料庫物件</param>
        /// <param name="locale">用於選擇語系欄位的語系代碼</param>
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

        /// <summary>
        /// 從指定表讀 Id、Name，預設依 Id 排序。
        /// </summary>
        /// <param name="table">查詢或異動所用的資料表名稱，由可信設定提供</param>
        /// <param name="db">要沿用的資料庫物件</param>
        /// <param name="sort">查詢使用的排序片段，由可信設定提供</param>
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

        /// <summary>
        /// 從指定表讀選項及擴充欄位，預設依 Id 排序。
        /// </summary>
        /// <param name="table">查詢或異動所用的資料表名稱，由可信設定提供</param>
        /// <param name="extFid">要讀為擴充值的資料庫欄位名稱</param>
        /// <param name="db">要沿用的資料庫物件</param>
        /// <param name="sort">查詢使用的排序片段，由可信設定提供</param>
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

        /// <summary>
        /// 查詢多個 XpCode 分類，Ext 帶回 Type；分類清單直接加引號組合。
        /// </summary>
        /// <param name="types">要查詢的代碼分類清單</param>
        /// <param name="db">要沿用的資料庫物件</param>
        /// <param name="locale">用於選擇語系欄位的語系代碼</param>
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

        /// <summary>
        /// 在清單首項原地加入空識別碼選項；輸入 null 時建立清單。
        /// </summary>
        /// <param name="codes">要處理的選項清單</param>
        /// <param name="plsSelect">空值選項要顯示的提示文字</param>
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

        /// <summary>
        /// 取得 XpUser 的使用者選項。
        /// </summary>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>> UsersA(Db? db = null)
        {
            return await TableToCodesA("dbo.XpUser", db);
        }

        /// <summary>
        /// 取得依 Sort 排列的 XpProg 選項。
        /// </summary>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>> ProgsA(Db? db = null)
        {
            return await TableToCodesA("dbo.XpProg", db, "Sort");
        }

        /// <summary>
        /// 取得角色選項，依 RoleAllCond 排除指定的全部人角色。
        /// </summary>
        /// <param name="db">要沿用的資料庫物件</param>
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

        /// <summary>
        /// 取得 xAuthRange 分類的資料範圍選項。
        /// </summary>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>> AuthRangesA(Db? db = null)
        {
            return await TypeToCodesA(AuthRange, db);
        }

        /// <summary>
        /// 取得 XpFlow 的流程選項。
        /// </summary>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>> XpFlowA(Db? db = null)
        {
            return await TableToCodesA("XpFlow", db);
        }

        /// <summary>
        /// 連接部門與角色，顯示為部門名稱-角色名稱。
        /// </summary>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>> DeptRolesA(Db? db = null)
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

        /// <summary>
        /// 取得依 Sort 排列的單層作業選項。
        /// </summary>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>> Progs1A(Db? db = null)
        {
            return await TableToCodesA("dbo.XpProg", db, "Sort");
        }

        /// <summary>
        /// 依 MenuGroup 分類及作業 Sort 排序取得作業選項。
        /// </summary>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>> Progs2A(Db? db = null)
        {
            var sql = $@"
select p.Id, p.Name as Str
from dbo.XpProg p
join dbo.XpCode c on c.Type='MenuGroup' and p.MenuGroup=c.Value
order by c.Sort, p.Sort";
            return await SqlToCodesA(sql, db);
        }

        /// <summary>
        /// 取得依 Sort 排列的部門選項。
        /// </summary>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>> DeptsA(Db? db = null)
        {
            return await TableToCodesA("dbo.XpDept", db, "Sort");
        }

        /// <summary>
        /// 從擴充代碼中取簽核分類的同意與退回項目；無有效清單時可能拋出。
        /// </summary>
        /// <param name="rows">要處理的資料列清單</param>
        public static List<IdStrDto>? CodesToSignStatuses(List<IdStrExtDto>? rows)
        {
            return FilterByExt(rows, SignStatus)!
                .Where(a => a.Id is SignStatusEstr.Agree or SignStatusEstr.Back)
                .ToList();
        }

        /// <summary>
        /// 取得簽核狀態；forSign 為 true 時只取 Ext=1 的可簽項目。
        /// </summary>
        /// <param name="forSign">是否只保留可供簽核操作選擇的狀態</param>
        /// <param name="db">要沿用的資料庫物件</param>
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

        /// <summary>
        /// 依 Ext 篩選並轉為 Id、Str 清單；來源 null 或空時回傳 null。
        /// </summary>
        /// <param name="rows">要處理的資料列清單</param>
        /// <param name="value">要使用的輸入值</param>
        public static List<IdStrDto>? FilterByExt(List<IdStrExtDto>? rows, string value)
        {
            return (rows == null || rows.Count == 0)
                ? null
                : rows.Where(a => a.Ext == value)
                    .Select(a => new IdStrDto { Id = a.Id, Str = a.Str })
                    .ToList();
        }

    }//class
}

using Base.Models;
using Base.Services;
using BaseFlow.Enums;

namespace BaseFlow.Services
{
    //for dropdown input
    /// <summary>從共用資料表與代碼取得簽核畫面的選項。</summary>
    public static class _FlowCode
    {
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
        /// 取得 XpFlow 的流程選項。
        /// </summary>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>> XpFlowA(Db? db = null)
        {
            return await _Code.TableToCodesA("XpFlow", db);
        }

        /// <summary>
        /// 從擴充代碼中取簽核分類的同意與退回項目；無有效清單時可能拋出。
        /// </summary>
        /// <param name="rows">要處理的資料列清單</param>
        public static List<IdStrDto>? CodesToSignStatuses(List<IdStrExtDto>? rows)
        {
            return _Code.FilterByExt(rows, SignStatus)!
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
                return await _Code.SqlToCodesA(sql, db);
            }
            else
            {
                return await _Code.TypeToCodesA(SignStatus, db);
            }
        }

        #region master table to codes
        /// <summary>讀取專案選項。</summary>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>?> ProjectsA(Db? db = null)
        {
            return await _Code.TableToCodesA("Project", db);
        }
        /// <summary>讀取使用者選項。</summary>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>?> UsersA(Db? db = null)
        {
            return await _Code.TableToCodesA("XpUser", db);
        }
        /// <summary>讀取部門選項。</summary>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>?> DeptsA(Db? db = null)
        {
            return await _Code.TableToCodesA("XpDept", db);
        }
        /// <summary>讀取角色選項。</summary>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>?> RolesA(Db? db = null)
        {
            return await _Code.TableToCodesA("XpRole", db);
        }

        /*
        public static async Task<List<IdStrDto>> GetProgsA(string locale0, Db? db = null)
        {
            //return TableToList("XpProg", db);
            var sql = $@"
select 
    p.Id, (case when p.AuthRow=1 then '*' else '' end)+c.Name_{locale0} as Str
from dbo.XpProg p
join dbo.XpCode c on c.Type='Menu' and p.Code=c.Value
order by p.Sort";
            return await SqlToListA(sql, db);
        }
        */

        /// <summary>讀取流程選項。</summary>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>?> FlowsA(Db? db = null)
        {
            return await _Code.TableToCodesA("XpFlow", db);
        }
        #endregion

        #region get from XpCode table
        /// <summary>讀取流程線來源類型選項。</summary>
        /// <param name="locale0">要用於查詢選項名稱的語系代碼</param>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>?> LineFromTypesA(string locale0, Db? db = null)
        {
            return await _Code.TypeToCodesA("LineFromType", db, locale0);
        }
        /// <summary>讀取資料權限範圍選項。</summary>
        /// <param name="locale0">要用於查詢選項名稱的語系代碼</param>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>?> AuthRangesA(string locale0, Db? db = null)
        {
            return await _Code.TypeToCodesA("AuthRange", db, locale0);
        }
        /// <summary>讀取語言程度選項。</summary>
        /// <param name="locale0">要用於查詢選項名稱的語系代碼</param>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>?> LangLevelsA(string locale0, Db? db = null)
        {
            return await _Code.TypeToCodesA("LangLevel", db, locale0);
        }
        /// <summary>讀取假別選項。</summary>
        /// <param name="locale0">要用於查詢選項名稱的語系代碼</param>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>?> LeaveTypesA(string locale0, Db? db = null)
        {
            return await _Code.TypeToCodesA("LeaveType", db, locale0);
        }
        //all xpCode rows
        /// <summary>讀取流程狀態選項。</summary>
        /// <param name="locale0">要用於查詢選項名稱的語系代碼</param>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>?> FlowStatusesA(string locale0, Db? db = null)
        {
            return await _Code.TypeToCodesA("FlowStatus", db, locale0);
        }
        /// <summary>讀取簽核狀態選項。</summary>
        /// <param name="locale0">要用於查詢選項名稱的語系代碼</param>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>?> SignStatusesA(string locale0, Db? db = null)
        {
            return await _Code.TypeToCodesA("SignStatus", db, locale0);
        }
        //ext=1 only for FlowSign Form
        /// <summary>讀取供簽核使用的狀態選項。</summary>
        /// <param name="locale0">用來組合 Name_語系 欄名的語系代碼，由可信設定提供。</param>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>?> SignStatuses2A(string locale0, Db? db = null)
        {
            var sql = $@"
select 
    Value as Id, Name_{locale0} as Str
from dbo.XpCode
where Type='SignStatus'
and Ext='1'
order by Sort";
            return await _Code.SqlToCodesA(sql, db);
        }
        #endregion

        #region for flow
        /// <summary>讀取流程節點類型選項。</summary>
        /// <param name="locale">用於選擇語系欄位的語系代碼</param>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>?> NodeTypesA(string locale = "", Db? db = null)
        {
            return await _Code.TypeToCodesA("NodeType", db, locale);
        }
        /// <summary>讀取簽核者類型選項。</summary>
        /// <param name="locale">用於選擇語系欄位的語系代碼</param>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>?> SignerTypesA(string locale = "", Db? db = null)
        {
            return await _Code.TypeToCodesA("SignerType", db, locale);
        }
        /// <summary>讀取條件的且／或運算選項。</summary>
        /// <param name="locale">用於選擇語系欄位的語系代碼</param>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>?> AndOrsA(string locale = "", Db? db = null)
        {
            return await _Code.TypeToCodesA("AndOr", db, locale);
        }
        /// <summary>讀取流程線條件運算子選項。</summary>
        /// <param name="locale">用於選擇語系欄位的語系代碼</param>
        /// <param name="db">要沿用的資料庫物件</param>
        public static async Task<List<IdStrDto>?> LineOpsA(string locale, Db? db = null)
        {
            return await _Code.TypeToCodesA("LineOp", db, locale);
        }

        /*
        public static List<IdStrDto> GetSignTypes()
        {
            return new List<IdStrDto>()
            {
                new IdStrDto(){ Id = "Y", Str = "同意" },
                new IdStrDto(){ Id = "N", Str = "不同意" },
            };
        }
        */
        #endregion

        #region others
        /// <summary>回傳 Select1、Select2 兩個固定下拉選項。</summary>
        public static List<IdStrDto> GetSelects()
        {
            return new List<IdStrDto>(){
                new IdStrDto(){ Id="1", Str="Select1"},
                new IdStrDto(){ Id="2", Str="Select2"},
            };
        }
        /// <summary>回傳 Radio1、Radio2 兩個固定單選選項。</summary>
        public static List<IdStrDto> GetRadios()
        {
            return new List<IdStrDto>(){
                new IdStrDto(){ Id="1", Str="Radio1"},
                new IdStrDto(){ Id="2", Str="Radio2"},
            };
        }
        #endregion

        /*
        private static async Task<List<IdStrDto>> TableToListA(string table, Db? db = null)
        {
            var sql = string.Format(@"
select 
    Id, [Name] as Str
from dbo.[{0}]
order by Id
", table);
            return await SqlToListA(sql, db);
        }

        //get codes from sql 
        private static async Task<List<IdStrDto>> SqlToListA(string sql, Db? db = null)
        {
            var emptyDb = false;
            _Fun.CheckOpenDb(ref db, ref emptyDb);

            var rows = await db.GetModelsA<IdStrDto>(sql);
            await _Fun.CheckCloseDbA(db, emptyDb);
            return rows;
        }

        //get code table rows
        private static async Task<List<IdStrDto>> TypeToListA(string locale0, string type, Db? db = null)
        {
            var sql = $@"
select 
    Value as Id, Name_{locale0} as Str
from dbo.XpCode
where Type='{type}'
order by Sort";
            return await SqlToListA(sql, db);           
        }

        public static string GetValue(XpCode row, string locale)
        {
            var name = "Name_" + locale;
            //return _Linq.FnGetValue<XpCode>(name).ToString();
            return _Model.GetValue<XpCode>(row, name)!.ToString()!;
        }

        public static List<IdStrExtModel> GetCodeExts(string type, Db? db = null)
        {
            var emptyDb = (db == null);
            if (emptyDb)
                db = new Db();

            var sql = string.Format(@"
select 
    Value as Id, Name as Str, Ext
from dbo.XpCode
where Type='{0}'
and Ext='0'
order by Sort
", type);
            var rows = db.GetModels<IdStrExtModel>(sql);
            if (emptyDb)
                db.Dispose();
            return rows;
        }
        */

    }//class
}

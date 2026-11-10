using Base.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// 清單組合、空值判斷與選項查找工具。
    /// </summary>
    public class _List
    {
        /// <summary>
        /// 取得 1 到 12 月的識別碼與中文顯示文字。
        /// </summary>
        public static List<IdStrDto> GetMonths()
        {
            return Enumerable.Range(1, 12)
                .Select(i => new IdStrDto { Id = i.ToString(), Str = i + "月" })
                .ToList();
        }

        /*
        /// <summary>
        /// sql string to list for select input
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static async Task<List<IdStrDto>?> SqlToListA(string sql, Db? db = null)
        {
            var newDb = _Db.CheckOpenDb(ref db);
            var rows = await db!.GetModelsA<IdStrDto>(sql);
            await _Db.CheckCloseDbA(db, newDb);
            return rows;
        }

        /// <summary>
        /// get code table rows
        /// type to list for select input
        /// </summary>
        /// <param name="type"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static async Task<List<IdStrDto>?> TypeToListA(string type, Db? db = null)
        {
            var sql = $@"
select Value as Id, Name as Str
from dbo.XpCode
where Type='{type}'
order by Sort";
            return await SqlToListA(sql, db);
        }

        public static async Task<List<IdStrDto>?> TableToListA(string table, Db? db = null)
        {
            var sql = $@"
select Id, Name as Str
from dbo.[{table}]
order by Id)
";
            return await SqlToListA(sql, db);
        }
        */

        /// <summary>
        /// 以分隔符連接清單，可加單引號；不進行 SQL 跳脫。
        /// </summary>
        /// <param name="list">要處理的輸入清單</param>
        /// <param name="quote">是否加上單引號</param>
        /// <param name="sep">用於連接或拆分項目的分隔符號</param>
        /// <returns></returns>
        public static string ToStr(List<string> list, bool quote = false, string sep = ",")
        {
            var result = (list == null || list.Count == 0)
                ? "" : string.Join(sep, list);
            return (quote)
                ? "'" + result.Replace(sep, "'" + sep + "'") + "'"
                : result;
        }

        /// <summary>
        /// 以分隔符連接清單，可加單引號；不進行 SQL 跳脫。
        /// </summary>
        /// <param name="list">要處理的輸入清單</param>
        /// <param name="quote">是否替各項加上單引號</param>
        /// <param name="sep">用於連接或拆分項目的分隔符號</param>
        public static string ToStr(List<int> list, bool quote, string sep = ",")
        {
            var result = (list == null || list.Count == 0)
                ? "" : string.Join(sep, list);
            return (quote)
                ? "'" + result.Replace(sep, "'" + sep + "'") + "'"
                : result;
        }

        /// <summary>
        /// 以英數、底線與逗號規則檢查鍵清單，可記錄失敗原因。
        /// </summary>
        /// <param name="list">要依鍵值規則檢查的清單。</param>
        /// <param name="logError">發生錯誤時是否寫入記錄</param>
        public static bool CheckKey(List<string> list, bool logError = true)
        {
            return _Str.CheckKey(ToStr(list), logError);
        }

        /// <summary>
        /// 合併兩個可空清單，不去重；結果無項目時回傳 null。
        /// </summary>
        /// <param name="list1">要合併的第一個清單</param>
        /// <param name="list2">要合併的第二個清單</param>
        public static List<string>? Concat(List<string?>? list1, List<string>? list2)
        {
            var list = new List<string>();
            if (list1 != null && list1.Count > 0)
                list.AddRange(list1!);
            if (list2 != null && list2.Count > 0)
                list.AddRange(list2);
            return (list.Count == 0) ? null : list;
        }

        /*
        public static List<IdStrDto> CodesAddEmpty(List<IdStrDto>? codes, string plsSelect)
        {
            codes ??= [];
            codes.Insert(0, new IdStrDto()
            {
                Id = "",
                Str = plsSelect,
            });
            return codes;
        }
        */

        /// <summary>
        /// 依顯示文字找第一個識別碼，找不到時回傳 null。
        /// </summary>
        /// <param name="codes">要處理的選項清單</param>
        /// <param name="str">要處理的來源字串</param>
        public static string? StrToId(List<IdStrDto>? codes, string str)
        {
            if (codes == null || codes.Count == 0)
                return null;

            return codes.Where(a => a.Str == str)
                .Select(a => a.Id)
                .FirstOrDefault();
        }

        /// <summary>
        /// 判斷清單為 null 或沒有項目。
        /// </summary>
        /// <param name="rows">要處理的資料列清單</param>
        public static bool IsEmpty<T>(List<T>? rows) where T : class
        {
            return rows == null || rows.Count == 0;
        }
        /// <summary>
        /// 判斷清單非 null 且含有項目。
        /// </summary>
        /// <param name="rows">要處理的資料列清單</param>
        public static bool NotEmpty<T>(List<T>? rows) where T : class
        {
            return !IsEmpty(rows);
        }
    }//class
}

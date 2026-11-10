using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// 讀取使用者與角色等關聯資料的選項值。
    /// </summary>
    public class _ModeUR
    {
        /// <summary>
        /// 依 key 查詢 Id 與 Str；表名、資料欄與條件欄直接組入 SQL，須由可信設定提供。
        /// </summary>
        /// <param name="key">要與指定條件欄比對的值。</param>
        /// <param name="db">要沿用的資料庫物件</param>
        /// <param name="table">查詢或異動所用的資料表名稱，由可信設定提供</param>
        /// <param name="dataFid">要選為 Str 的資料欄名，由可信設定提供</param>
        /// <param name="whereFid">用於比對鍵值的資料欄名，由可信設定提供</param>
        /// <returns></returns>
        public static async Task<JArray?> GetValueRows(string table, string dataFid, string whereFid, string key, Db? db = null)
        {
            var sql = $@"
select Id, Str={dataFid}
from {table} 
where {whereFid}=@Id
";
            return await _Db.GetRowsA(sql, new() { "Id", key }, db);
        }

    }//class
}

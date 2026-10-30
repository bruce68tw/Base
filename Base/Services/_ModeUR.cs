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
        /// <param name="table1">master table name(主檔)</param>
        /// <param name="table2">slave table name(資料檔)</param>
        /// <param name="joinFid2">table2 fid for join</param>
        /// <param name="whereFid2">table2 fid for where</param>
        /// <param name="key"></param>
        /// <param name="db"></param>
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

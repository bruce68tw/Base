using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// URM(user role mode), 未來會有多種mode, 所以mode在前
    /// role的部分為多筆, 以checkbox輸入
    /// </summary>
    public class _ModeUR
    {
        /// <summary>
        /// (URM) get child rows
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
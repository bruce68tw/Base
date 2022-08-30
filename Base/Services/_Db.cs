using Base.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// package Db class to static 
    /// </summary>
    public class _Db
    {
        #region GetJson(s)
        public static async Task<JObject> GetJsonA(string sql, List<object> args = null, string dbStr = "")
        {
            var rows = await GetJsonsA(sql, args, dbStr);
            return (rows == null || rows.Count == 0) ? null : (JObject)rows[0];
        }

        public static async Task<JArray> GetJsonsA(string sql, List<object> args = null, string dbStr = "")
        {
            return await new Db(dbStr).GetJsonsA(sql, args);
        }
        #endregion        

        #region GetModel(s)
        public static async Task<T> GetModelA<T>(string sql, List<object> args = null, string dbStr = "")
        {
            var rows = await GetModelsA<T>(sql, args, dbStr);
            return (rows == null || rows.Count == 0) ? default(T) : rows[0];
        }
        public static async Task<List<T>> GetModelsA<T>(string sql, List<object> args = null, string dbStr = "")
        {
            return await new Db(dbStr).GetModelsA<T>(sql, args);
        }
        #endregion

        #region get string
        public static async Task<string> GetStrA(string sql, List<object> args = null, string dbStr = "")
        {
            return await new Db(dbStr).GetStrA(sql, args);
        }

        public static async Task<List<string>> GetStrsA(string sql, List<object> args = null, string dbStr = "")
        {
            return await new Db(dbStr).GetStrsA(sql, args);
        }
        #endregion

        #region get List<IdStrDto>
        public static async Task<List<IdStrDto>> TableToCodesA(string table, Db db = null)
        {
            var sql = string.Format(@"
select 
    Id, Name as Str
from dbo.[{0}]
order by Id
", table);
            return await SqlToCodesA(sql, db);
        }

        //get code table rows
        public static async Task<List<IdStrDto>> TypeToCodesA(string type, Db db = null, string locale = "")
        {
            var name = string.IsNullOrEmpty(locale) ? "Name" : "Name_" + locale;
            var sql = $@"
select 
    Value as Id, {name} as Str
from dbo.XpCode
where Type='{type}'
order by Sort";
            return await SqlToCodesA(sql, db);
        }

        //get codes from sql 
        public static async Task<List<IdStrDto>> SqlToCodesA(string sql, Db db = null)
        {
            var emptyDb = false;
            _Fun.CheckOpenDb(ref db, ref emptyDb);

            var rows = await db.GetModelsA<IdStrDto>(sql);
            await _Fun.CheckCloseDbA(db, emptyDb);
            return rows;
        }
        #endregion

        //update
        public static async Task<int> ExecSqlA(string sql, List<object> args = null, string dbStr = "")
        {
            return await new Db(dbStr).ExecSqlA(sql, args);
        }

        //set row Status column to true/false
        public static async Task<bool> SetRowStatusA(string table, string kid, object kvalue, bool status, string statusId = "Status", string where = "", string dbStr = "")
        {
            return await new Db(dbStr).SetRowStatus(table, kid, kvalue, status, statusId, where);
        }

    }//class
}
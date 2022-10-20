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
        /// <summary>
        /// check and open db
        /// </summary>
        /// <param name="db"></param>
        /// <param name="hasDb"></param>
        /// <param name="dbStr"></param>
        /// <returns>true(new open db)</returns>
        public static bool CheckOpenDb(ref Db db, string dbStr = "")
        {
            var newDb = (db == null);
            if (newDb)
                db = new Db(dbStr);
            return newDb;
        }

        /// <summary>
        /// check and close db
        /// </summary>
        /// <param name="db"></param>
        /// <param name="newDb">true(new open db)</param>
        public static async Task CheckCloseDbA(Db db, bool newDb)
        {
            if (newDb)
                await db.DisposeAsync();
        }

        #region GetJson(s)
        public static async Task<JObject> GetJsonA(string sql, List<object> args = null, Db db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await GetJsonsA(sql, args, db);
            await CheckCloseDbA(db, newDb);
            return (rows == null || rows.Count == 0) 
                ? null : (JObject)rows[0];
        }

        public static async Task<JArray> GetJsonsA(string sql, List<object> args = null, Db db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await db.GetJsonsA(sql, args);
            await CheckCloseDbA(db, newDb);
            return rows;
        }
        #endregion        

        #region GetModel(s)
        public static async Task<T> GetModelA<T>(string sql, List<object> args = null, Db db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await GetModelsA<T>(sql, args, db);
            await CheckCloseDbA(db, newDb);
            return (rows == null || rows.Count == 0) ? default(T) : rows[0];
        }
        public static async Task<List<T>> GetModelsA<T>(string sql, List<object> args = null, Db db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await db.GetModelsA<T>(sql, args);
            await CheckCloseDbA(db, newDb);
            return rows;
        }
        #endregion

        #region get string
        public static async Task<string> GetStrA(string sql, List<object> args = null, Db db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var result = await db.GetStrA(sql, args);
            await CheckCloseDbA(db, newDb);
            return result;
        }

        public static async Task<List<string>> GetStrsA(string sql, List<object> args = null, Db db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var result = await db.GetStrsA(sql, args);
            await CheckCloseDbA(db, newDb);
            return result;
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
            var newDb = CheckOpenDb(ref db);
            var rows = await db.GetModelsA<IdStrDto>(sql);
            await CheckCloseDbA(db, newDb);
            return rows;
        }
        #endregion

        //update
        public static async Task<int> ExecSqlA(string sql, List<object> args = null, Db db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var result = await db.ExecSqlA(sql, args);
            await CheckCloseDbA(db, newDb);
            return result;
            //return await new Db(dbStr).ExecSqlA(sql, args);
        }

        /*
        //set row Status column to true/false
        public static async Task<bool> SetRowStatusA(string table, string kid, object kvalue, bool status, string statusId = "Status", string where = "", string dbStr = "")
        {
            return await new Db(dbStr).SetRowStatus(table, kid, kvalue, status, statusId, where);
        }
        */

    }//class
}
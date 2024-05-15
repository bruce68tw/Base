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
        public static bool CheckOpenDb(ref Db? db, string dbStr = "")
        {
            var newDb = (db == null);
            if (newDb) db = new Db(dbStr);
            return newDb;
        }

        /// <summary>
        /// check and close db
        /// </summary>
        /// <param name="db"></param>
        /// <param name="newDb">true(new open db)</param>
        public static async Task CheckCloseDbA(Db db, bool newDb)
        {
            if (newDb) await db.DisposeAsync();
        }

        #region GetJson(s)
        /// <summary>
        /// get json
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="args">ex: new() { "Id", id }</param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static async Task<JObject?> GetJsonA(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await GetJsonsA(sql, args, db);
            await CheckCloseDbA(db!, newDb);
            return (rows == null || rows.Count == 0) 
                ? null : (JObject)rows[0];
        }

        public static async Task<JArray?> GetJsonsA(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await db!.GetJsonsA(sql, args);
            await CheckCloseDbA(db, newDb);
            return rows;
        }
        #endregion        

        #region GetModel(s)
        public static async Task<T?> GetModelA<T>(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await GetModelsA<T>(sql, args, db);
            await CheckCloseDbA(db!, newDb);
            return (rows == null || rows.Count == 0) 
                ? default : rows[0];
        }
        public static async Task<List<T>?> GetModelsA<T>(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await db!.GetModelsA<T>(sql, args);
            await CheckCloseDbA(db, newDb);
            return rows;
        }
        #endregion

        #region get string
        /// <summary>
        /// get string
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="args">ex: new() { "Id", id }</param>
        /// <param name="db"></param>
        /// <returns>return null if not found</returns>
        public static async Task<string?> GetStrA(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var result = await db!.GetStrA(sql, args);
            await CheckCloseDbA(db, newDb);
            return result;
        }

        public static async Task<List<string>?> GetStrsA(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var result = await db!.GetStrsA(sql, args);
            await CheckCloseDbA(db, newDb);
            return result;
        }
        #endregion

        #region get int
        /// <summary>
        /// get string
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="args">ex: new() { "Id", id }</param>
        /// <param name="db"></param>
        /// <returns>return null if not found</returns>
        public static async Task<int?> GetIntA(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var result = await db!.GetIntA(sql, args);
            await CheckCloseDbA(db, newDb);
            return result;
        }

        public static async Task<List<int>?> GetIntsA(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var result = await db!.GetIntsA(sql, args);
            await CheckCloseDbA(db, newDb);
            return result;
        }
        #endregion

        #region get List<IdStrDto>
        public static async Task<List<IdStrDto>?> TableToCodesA(string table, Db? db = null)
        {
            var sql = @$"
select Id, Name as Str
from dbo.[{table}]
order by Id";
            return await SqlToCodesA(sql, db);
        }

        //加上排序欄位
        public static async Task<List<IdStrDto>?> TableToCodes2A(string table, string sort, Db? db = null)
        {
            var sql = @$"
select Id, Name as Str
from dbo.[{table}]
order by {sort}";
            return await SqlToCodesA(sql, db);
        }

        public static async Task<List<IdStrExtDto>?> TableToCodeExtsA(string table, string extFid, Db? db = null)
        {
            var sql = @$"
select Id, Name as Str, {extFid} as Ext
from dbo.[{table}]
order by Id";
            return await SqlToCodeExtsA(sql, db);
        }

        //加上排序欄位
        public static async Task<List<IdStrExtDto>?> TableToCodeExts2A(string table, string extFid, string sort, Db? db = null)
        {
            var sql = @$"
select Id, Name as Str, {extFid} as Ext
from dbo.[{table}]
order by {sort}";
            return await SqlToCodeExtsA(sql, db);
        }

        public static async Task<List<IdStrExt2Dto>?> TableToCodeExt2sA(string table, string extFid, string ext2Fid, Db? db = null)
        {
            var sql = @$"
select Id, Name as Str, {extFid} as Ext, {ext2Fid} as Ext2
from dbo.[{table}]
order by Id";
            return await SqlToCodeExt2sA(sql, db);
        }

        //get code table rows
        public static async Task<List<IdStrDto>?> TypeToCodesA(string type, Db? db = null, string locale = "")
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
        public static async Task<List<IdStrDto>?> SqlToCodesA(string sql, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await db!.GetModelsA<IdStrDto>(sql);
            await CheckCloseDbA(db, newDb);
            return rows;
        }
        public static async Task<List<IdStrExtDto>?> SqlToCodeExtsA(string sql, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await db!.GetModelsA<IdStrExtDto>(sql);
            await CheckCloseDbA(db, newDb);
            return rows;
        }
        public static async Task<List<IdStrExt2Dto>?> SqlToCodeExt2sA(string sql, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await db!.GetModelsA<IdStrExt2Dto>(sql);
            await CheckCloseDbA(db, newDb);
            return rows;
        }
        #endregion

        //update
        //return affected rows, -1 means error
        public static async Task<int> ExecSqlA(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var result = await db!.ExecSqlA(sql, args);
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
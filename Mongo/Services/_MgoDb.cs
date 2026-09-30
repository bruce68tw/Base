using Base.Models;
using Mongo.Enums;
using Mongo.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Mongo.Services
{
    /// <summary>
    /// package Db class to static 
    /// </summary>
    public class _MgoDb
    {
        /// <summary>
        /// check and open db
        /// </summary>
        /// <param name="db"></param>
        /// <param name="hasDb"></param>
        /// <param name="dbStr"></param>
        /// <returns>true(new open db)</returns>
        public static bool CheckOpenDb(ref MgoDb? db, string dbStr = "")
        {
            var newDb = (db == null);
            if (newDb) db = new MgoDb(dbStr);
            return newDb;
        }

        /// <summary>
        /// check and close db
        /// </summary>
        /// <param name="db"></param>
        /// <param name="newDb">true(new open db)</param>
        public static void CheckCloseDb(MgoDb db, bool newDb)
        {
            if (newDb) db.Dispose();
        }

        public static bool Update(string table, string rowId, JObject row, MgoDb? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var ok = db!.Update(table, rowId, row);
            CheckCloseDb(db!, newDb);
            return ok;
        }
        public static bool UpdateByFilter(string table, FilterDefinition<BsonDocument> filter, JObject row, MgoDb? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var ok = db!.UpdateByFilter(table, filter, row);
            CheckCloseDb(db!, newDb);
            return ok;
        }

        #region GetRow(s)
        /// <summary>
        /// get json
        /// </summary>
        /// <param name="table"></param>
        /// <param name="args">ex: new() { "Id", id }</param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static async Task<JObject?> GetRowA(string table, List<MgoQitemDto>? qitems = null, string sorts = "", MgoDb? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await GetRowsA(table, qitems, sorts, db);
            CheckCloseDb(db!, newDb);
            return (rows == null || rows.Count == 0) 
                ? null : (JObject)rows[0];
        }

        public static async Task<JArray?> GetRowsA(string table, List<MgoQitemDto>? qitems = null, string sorts = "", MgoDb? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await db!.GetRowsA(table, qitems, sorts);
            CheckCloseDb(db, newDb);
            return rows;
        }
        #endregion        

        public static async Task<List<BsonDocument>?> GetBsonsA(string table, List<MgoQitemDto>? qitems = null, string sorts = "", int? maxCount = null, MgoDb? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await db!.GetBsonsA(table, qitems, sorts, maxCount);
            CheckCloseDb(db, newDb);
            return rows;
        }

        public static async Task<List<BsonDocument>?> GetBsonsByFilterA(string table, FilterDefinition<BsonDocument> filter, string sorts = "", int? maxCount = null, MgoDb? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await db!.GetBsonsByFilterA(table, filter, sorts, maxCount);
            CheckCloseDb(db, newDb);
            return rows;
        }

        #region GetModel(s)
        public async Task<IdStrDto?> GetIdStrA(string table, List<MgoQitemDto>? qitems = null, MgoDb? db = null)
        {
            return await db.GetModelA<IdStrDto>(table, qitems);
        }
        public async Task<List<IdStrDto>?> GetIdStrsA(string table, List<MgoQitemDto>? qitems = null, MgoDb? db = null)
        {
            return await db.GetModelsA<IdStrDto>(table, qitems);
        }
        public static async Task<T?> GetModelA<T>(string table, List<MgoQitemDto>? qitems = null, string sorts = "", MgoDb? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await db.GetModelsA<T>(table, qitems, sorts);
            CheckCloseDb(db!, newDb);
            return (rows == null || rows.Count == 0) 
                ? default : rows[0];
        }
        public static async Task<List<T>?> GetModelsA<T>(string table, List<MgoQitemDto>? qitems = null, string sorts = "", MgoDb? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await db!.GetModelsA<T>(table, qitems, sorts);
            CheckCloseDb(db!, newDb);
            return rows;
        }
        #endregion

        public static FilterDefinition<BsonDocument> JsonStrToFilter(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
                return Builders<BsonDocument>.Filter.Empty;

            //移除空白欄位, 避免用空值當作查詢條件
            var json = JObject.Parse(jsonStr);
            foreach (var prop in json.Properties().ToList())
            {
                if (prop.Value.Type == JTokenType.String && string.IsNullOrWhiteSpace(prop.Value.ToString()))
                    prop.Remove();
            }

            return (json.Count == 0)
                ? Builders<BsonDocument>.Filter.Empty
                : json.ToString(Newtonsoft.Json.Formatting.None);
        }

        public static FilterDefinition<BsonDocument> PairToFilter(string fid, string value)
        {
            var qitems = PairToQitems(fid, value);
            return QitemsToFilter(qitems);
        }
        public static List<MgoQitemDto> PairToQitems(string fid, string value)
        {
            List<MgoQitemDto> qitems =
            [
                new MgoQitemDto()
                {
                    Fid = fid,
                    Value = value,
                }
            ];
            return qitems;
        }
        public static FilterDefinition<BsonDocument> IdToFilter(string rowId)
        {
            return PairToFilter("_id", rowId);
        }

        public static FilterDefinition<BsonDocument> QitemsToFilter(List<MgoQitemDto>? qitems)
        {
            if (qitems == null || qitems.Count == 0)
                return Builders<BsonDocument>.Filter.Empty;

            var filters = new List<FilterDefinition<BsonDocument>>();
            foreach (var qitem in qitems)
            {
                if (string.IsNullOrEmpty(qitem.Fid) || string.IsNullOrEmpty(qitem.Value))
                    continue;

                var value = qitem.Value;
                var filter = qitem.Op switch
                {
                    MgoQitemOpEstr.Equal => Builders<BsonDocument>.Filter.Eq(qitem.Fid, value),
                    MgoQitemOpEstr.Like => Builders<BsonDocument>.Filter.Regex(qitem.Fid,
                        new BsonRegularExpression($"^{Regex.Escape(value)}", "i")),
                    MgoQitemOpEstr.NotLike => Builders<BsonDocument>.Filter.Not(
                        Builders<BsonDocument>.Filter.Regex(qitem.Fid,
                            new BsonRegularExpression($"^{Regex.Escape(value)}", "i"))),
                    MgoQitemOpEstr.In => GetInFilter(qitem.Fid, value),
                    MgoQitemOpEstr.Like2 => Builders<BsonDocument>.Filter.Regex(qitem.Fid,
                        new BsonRegularExpression(Regex.Escape(value), "i")),
                    _ => null,
                };
                if (filter != null)
                    filters.Add(filter);
            }

            return (filters.Count == 0)
                ? Builders<BsonDocument>.Filter.Empty
                : Builders<BsonDocument>.Filter.And(filters);
        }

        private static FilterDefinition<BsonDocument> GetInFilter(string fid, string value)
        {
            var values = value.Replace(" ", "").Replace("\r", "").Replace("\n", ",")
                .Split(',', StringSplitOptions.RemoveEmptyEntries);
            return Builders<BsonDocument>.Filter.In(fid, values);
        }

        /*
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

        public static async Task<List<int?>?> GetIntsA(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var result = await db!.GetIntsA(sql, args);
            await CheckCloseDbA(db, newDb);
            return result;
        }
        #endregion
        */

        #region get List<IdStrDto>
        /*
        /// <summary>
        /// table to xpCode
        /// </summary>
        /// <param name="table"></param>
        /// <param name="db"></param>
        /// <param name="order">ex: Name [desc]</param>
        /// <returns></returns>
        public static async Task<List<IdStrDto>?> TableToCodesA(string table, Db? db = null, string? order = null)
        {
            order ??= "Id";
            var sql = $@"
select Id, Name as Str
from dbo.[{table}]
order by {order}";
            return await SqlToCodesA(sql, null, db);
        }

        //加上排序欄位
        public static async Task<List<IdStrDto>?> TableToCodes2A(string table, string sort, Db? db = null)
        {
            var sql = @$"
select Id, Name as Str
from dbo.[{table}]
order by {sort}";
            return await SqlToCodesA(sql, null, db);
        }

        public static async Task<List<IdStrExtDto>?> TableToCodeExtsA(string table, string extFid, Db? db = null)
        {
            var sql = @$"
select Id, Name as Str, {extFid} as Ext
from dbo.[{table}]
order by Id";
            return await SqlToCodeExtsA(sql, null, db);
        }

        //加上排序欄位
        public static async Task<List<IdStrExtDto>?> TableToCodeExts2A(string table, string extFid, string sort, Db? db = null)
        {
            var sql = @$"
select Id, Name as Str, {extFid} as Ext
from dbo.[{table}]
order by {sort}";
            return await SqlToCodeExtsA(sql, null, db);
        }

        public static async Task<List<IdStrExt2Dto>?> TableToCodeExt2sA(string table, string extFid, string ext2Fid, Db? db = null)
        {
            var sql = @$"
select Id, Name as Str, {extFid} as Ext, {ext2Fid} as Ext2
from dbo.[{table}]
order by Id";
            return await SqlToCodeExt2sA(sql, null, db);
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
            return await SqlToCodesA(sql, null, db);
        }

        //get code table rows
        public static async Task<List<IdStrExtDto>?> TypesToCodesA(string[] types, Db? db = null, string locale = "")
        {
            var name = string.IsNullOrEmpty(locale) ? "Name" : "Name_" + locale;
            var sql = $@"
select 
    Value as Id, {name} as Str, Type as Ext
from dbo.XpCode
where Type in ({_Array.ToStr(types, true)})
order by Type, Sort";
            return await SqlToCodeExtsA(sql, null, db);
        }

        //get codes from sql 
        public static async Task<List<IdStrDto>?> SqlToCodesA(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await db!.GetModelsA<IdStrDto>(sql, args);
            await CheckCloseDbA(db, newDb);
            return rows;
        }
        public static async Task<List<IdStrExtDto>?> SqlToCodeExtsA(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await db!.GetModelsA<IdStrExtDto>(sql, args);
            await CheckCloseDbA(db, newDb);
            return rows;
        }
        public static async Task<List<IdStrExt2Dto>?> SqlToCodeExt2sA(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await db!.GetModelsA<IdStrExt2Dto>(sql, args);
            await CheckCloseDbA(db, newDb);
            return rows;
        }
        */
        #endregion

        /*
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
        */

        /*
        //set row Status column to true/false
        public static async Task<bool> SetRowStatusA(string table, string kid, object kvalue, bool status, string statusId = "Status", string where = "", string dbStr = "")
        {
            return await new Db(dbStr).SetRowStatus(table, kid, kvalue, status, statusId, where);
        }
        */

    }//class
}
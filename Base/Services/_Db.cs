using Base.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// Db 的便利包裝；參數為名稱、值交錯配對，自建 Db 於正常完成後釋放，傳入的 Db 由呼叫端管理。
    /// </summary>
    public class _Db
    {
        /// <summary>
        /// db 為 null 時只建立 Db 並回傳 true，不會立即開連線。
        /// </summary>
        public static bool CheckOpenDb(ref Db? db, string dbStr = "")
        {
            var newDb = (db == null);
            if (newDb) db = new Db(dbStr);
            return newDb;
        }

        /// <summary>
        /// 僅在 newDb 為 true 時非同步釋放 Db。
        /// </summary>
        public static async Task CheckCloseDbA(Db db, bool newDb)
        {
            if (newDb) await db.DisposeAsync();
        }

        #region GetRow(s)
        /// <summary>
        /// 取得第一列 JSON；無資料或失敗可為 null。
        /// </summary>
        public static async Task<JObject?> GetRowA(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await GetRowsA(sql, args, db);
            await CheckCloseDbA(db!, newDb);
            return (rows == null || rows.Count == 0) 
                ? null : (JObject)rows[0];
        }

        /// <summary>
        /// 取得多列 JSON；無資料或失敗可為 null。
        /// </summary>
        public static async Task<JArray?> GetRowsA(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await db!.GetRowsA(sql, args);
            await CheckCloseDbA(db, newDb);
            return rows;
        }
        #endregion        

        #region GetModel(s)
        /// <summary>
        /// 取得第一個識別碼與文字配對，無資料可為 null；此方法為實例方法。
        /// </summary>
        public async Task<IdStrDto?> GetIdStrA(string sql, List<object>? sqlArgs = null, Db? db = null)
        {
            return await GetModelA<IdStrDto>(sql, sqlArgs, db);
        }
        /// <summary>
        /// 取得識別碼與文字清單，無資料可為 null；此方法為實例方法。
        /// </summary>
        public async Task<List<IdStrDto>?> GetIdStrsA(string sql, List<object>? sqlArgs = null, Db? db = null)
        {
            return await GetModelsA<IdStrDto>(sql, sqlArgs, db);
        }
        /// <summary>
        /// 以同名屬性讀取首列模型，無資料回傳 default。
        /// </summary>
        public static async Task<T?> GetModelA<T>(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var rows = await GetModelsA<T>(sql, args, db);
            await CheckCloseDbA(db!, newDb);
            return (rows == null || rows.Count == 0) 
                ? default : rows[0];
        }
        /// <summary>
        /// 以同名屬性讀取模型清單，無資料或失敗可為 null。
        /// </summary>
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
        /// 取得首列首欄字串，無資料可為 null。
        /// </summary>
        public static async Task<string?> GetStrA(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var result = await db!.GetStrA(sql, args);
            await CheckCloseDbA(db, newDb);
            return result;
        }

        /// <summary>
        /// 取得首欄字串清單，無資料可為 null。
        /// </summary>
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
        /// 取得首列首欄可空整數。
        /// </summary>
        public static async Task<int?> GetIntA(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var result = await db!.GetIntA(sql, args);
            await CheckCloseDbA(db, newDb);
            return result;
        }

        /// <summary>
        /// 取得首欄可空整數清單。
        /// </summary>
        public static async Task<List<int?>?> GetIntsA(string sql, List<object>? args = null, Db? db = null)
        {
            var newDb = CheckOpenDb(ref db);
            var result = await db!.GetIntsA(sql, args);
            await CheckCloseDbA(db, newDb);
            return result;
        }
        #endregion

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

        //return affected rows, -1 means error
        /// <summary>
        /// 執行異動，沿用 Db 的筆數與 0、-1、-2 回傳規則；不自動建立交易。
        /// </summary>
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

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
        public static async Task<JObject> GetJsonAsync(string sql, List<object> args = null, string dbStr = "")
        {
            var rows = await GetJsonsAsync(sql, args, dbStr);
            return (rows == null || rows.Count == 0) ? null : (JObject)rows[0];
        }

        public static async Task<JArray> GetJsonsAsync(string sql, List<object> args = null, string dbStr = "")
        {
            return await new Db(dbStr).GetJsonsAsync(sql, args);
        }
        #endregion        

        #region GetModel(s)
        public static async Task<T> GetModelAsync<T>(string sql, List<object> args = null, string dbStr = "")
        {
            var rows = await GetModelsAsync<T>(sql, args, dbStr);
            return (rows == null || rows.Count == 0) ? default(T) : rows[0];
        }
        public static async Task<List<T>> GetModelsAsync<T>(string sql, List<object> args = null, string dbStr = "")
        {
            return await new Db(dbStr).GetModelsAsync<T>(sql, args);
        }
        #endregion

        #region others
        public static async Task<string> GetStrAsync(string sql, List<object> args = null, string dbStr = "")
        {
            return await new Db(dbStr).GetStrAsync(sql, args);
        }

        public static async Task<List<string>> GetStrsAsync(string sql, List<object> args = null, string dbStr = "")
        {
            return await new Db(dbStr).GetStrsAsync(sql, args);
        }
        #endregion

        //update
        public static async Task<int> ExecSqlAsync(string sql, List<object> args = null, string dbStr = "")
        {
            return await new Db(dbStr).ExecSqlAsync(sql, args);
        }

        //set row Status column to true/false
        public static async Task<bool> SetRowStatusAsync(string table, string kid, object kvalue, bool status, string statusId = "Status", string where = "", string dbStr = "")
        {
            return await new Db(dbStr).SetRowStatus(table, kid, kvalue, status, statusId, where);
        }

    }//class
}
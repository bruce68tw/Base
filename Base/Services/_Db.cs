using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Base.Services
{
    /// <summary>
    /// package Db class to static 
    /// </summary>
    public class _Db
    {
        #region GetJson(s)
        public static JObject GetJson(string sql, List<object> args = null, string dbStr = "")
        {
            var rows = GetJsons(sql, args, dbStr);
            return (rows == null || rows.Count == 0) ? null : (JObject)rows[0];
        }

        public static JArray GetJsons(string sql, List<object> args = null, string dbStr = "")
        {
            return new Db(dbStr).GetJsons(sql, args);
        }
        #endregion        

        #region GetModel(s)
        public static T GetModel<T>(string sql, List<object> args = null, string dbStr = "")
        {
            var rows = GetModels<T>(sql, args, dbStr);
            return (rows == null || rows.Count == 0) ? default(T) : rows[0];
        }
        public static List<T> GetModels<T>(string sql, List<object> args = null, string dbStr = "")
        {
            return new Db(dbStr).GetModels<T>(sql, args);
        }
        #endregion
        
        //update
        public static int ExecSql(string sql, List<object> args = null, string dbStr = "")
        {
            return new Db(dbStr).ExecSql(sql, args);
        }

        //set row Status column to true/false
        public static bool SetRowStatus(string table, string kid, object kvalue, bool status, string statusId = "Status", string where = "", string dbStr = "")
        {
            return new Db(dbStr).SetRowStatus(table, kid, kvalue, status, statusId, where);
        }

    }//class
}
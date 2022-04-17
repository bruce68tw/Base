using Base.Enums;
using Base.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// database read/write 
    /// </summary>
    public class Db : IAsyncDisposable
    {
        private DbConnection _conn = null;   //or will compile error !!
        private DbCommand _cmd = null;
        private DbTransaction _tran;

        //column mapping for update/insert, key-type-欄位序號
        private List<IdNumDto> _colMap = new();

        //status
        private bool _status;    //db status

        //sql args pairs, ex:"UserId","001",,,
        private List<object> _sqlArgs;

        private DateTime _now;      //for sql slow action
        private readonly string _dbStr;      //db connection string        
        private const int _dbSec = 600; //command second, can not too short

        /// <summary>
        /// constructor, new db, 
        /// note: if db server live but not access right, it still connect ok !!
        /// </summary>
        /// <param name="dbStr">db field name in config, default to "db"</param>
        /// <param name="di">for lightinject DI, for different database engine, use object type, or called must include LightInject !!</param>
        public Db(string dbStr = "")
        {
            //_dbStr: db field name at config, if length > 30, it will be connection string !!
            _dbStr = (dbStr == "") ? _Fun.Config.Db : dbStr;

            //_isTran = transaction;
            //InitDb(); //here !!
        }

        public DbConnection GetConnection()
        {
            return _conn;
        }

        /// <summary>
        /// initial db, when set command, it will initial Db auto
        /// if you need to operate reader(not by command), call this method by yourself !!
        /// </summary>
        /// <returns>error msg if any</returns>
        public async Task<bool> InitDbAsync()
        {
            if (_status)
                return true;

            //when _dbStr length > 30, treat it as connection string
            //var connStr = (_dbStr.Length > 30) ? _dbStr : _Config.GetDbStr(_dbStr);

            _conn = (DbConnection)_Fun.DiBox.GetService(typeof(DbConnection));
            //set connect string will get error when connecting state
            //if (_conn.ConnectionString == null || _conn.ConnectionString != _dbStr)
                _conn.ConnectionString = _dbStr;
            //_conn.State = ConnectionState.Connecting
            try
            {
                await _conn.OpenAsync();
                _status = true;
                return true;
            }
            catch (Exception ex)
            {
                //set instance variables
                _status = false;
                _conn = null;

                //not log db connection string for security reason
                await _Log.ErrorAsync("connect db error: " + ex.Message);
                return false;
            }
        }

        //must use ValueTask
        public async ValueTask DisposeAsync()
        {
            if (_cmd != null)
                await _cmd.DisposeAsync();
            if (_conn != null)
                await _conn.DisposeAsync();
        }

        /*
        /// <summary>
        /// check db connection is ready or not
        /// </summary>
        /// <returns></returns>
        public bool GetStatus()
        {
            return _status;
        }

        /// <summary>
        /// return command , PG can handle
        /// </summary>
        /// <returns></returns>
        public DbCommand GetCmd()
        {
            if (_cmd == null)
            {
                //_cmd = new IDbCommand();
                _cmd = (DbCommand)_Fun.DiBox.GetService(typeof(DbCommand));
                _cmd.Connection = _conn;
                _cmd.CommandTimeout = _dbSec;
            }
            return _cmd;
        }        
        */

        /// <summary>
        /// initial command and add parameters, array: field name, value...
        /// OleDb does not support named parameters !!
        /// sqlClient ExecuteReader will call sp_executesql & reused execute plan, increase performance
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlArgs"></param>
        /// <param name="dbSec"></param>
        /// <returns>error msg if any</returns>
        private async Task<bool> InitCmdAsync(string sql, List<object> sqlArgs = null, int dbSec = _dbSec)
        {
            if (!await InitDbAsync())
                return false;

            //set instance variables
            _sqlArgs = sqlArgs;

            //init command
            if (_cmd == null)
            {
                //_cmd = new SqlCommand();
                _cmd = (DbCommand)_Fun.DiBox.GetService(typeof(DbCommand));
                _cmd.Connection = _conn;
            }
            else
            {
                _cmd.Parameters.Clear();    //must clear parameters first
            }

            _cmd.CommandType = CommandType.Text;
            _cmd.CommandTimeout = dbSec;            

            //set parameters
            //OleDb does not support named parameters !!
            if (sqlArgs != null && sqlArgs.Count > 0)
            {
                for (var i = 0; i < sqlArgs.Count; i += 2)
                {
                    var arg = _cmd.CreateParameter();
                    arg.ParameterName = "@" + sqlArgs[i].ToString();
                    //can not use iif here, has different type
                    if (sqlArgs[i + 1] == null)
                        arg.Value = DBNull.Value;
                    else if (sqlArgs[i + 1].GetType() == typeof(DateTime))
                        arg.Value = (DateTime)sqlArgs[i + 1];
                    else
                        arg.Value = sqlArgs[i + 1].ToString();

                    _cmd.Parameters.Add(arg);
                }
            }
            _cmd.CommandText = sql;

            //log debug info
            if (_Str.NotEmpty(sql))
                await _Log.SqlAsync(GetSqlText(sql));
            return true;
        }


        /// <summary>
        /// when use bulk copy, need to handle reader directly !!
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlArgs"></param>
        /// <param name="dbSec"></param>
        /// <returns></returns>
        public async Task<IDataReader> GetReaderAsync(string sql, List<object> sqlArgs = null, int dbSec = _dbSec)
        {
            //init command
            if (!await InitCmdAsync(sql, sqlArgs, dbSec))
                return null;

            try
            {
                SetNow();
                var reader = await _cmd.ExecuteReaderAsync();
                await LogSlowSql(sql);
                SetColMap(reader);
                return reader;
            }
            catch (Exception ex)
            {
                await _Log.ErrorAsync($"_cmd.ExecuteReader() error: {GetSqlText(sql)}, {ex.Message}\n");
                return null;
            }
        }

        //get sql text by argValue[]
        private string GetSqlText(string sql)
        {           
            return sql + GetArgsText(_sqlArgs);
        }

        /// <summary>
        /// get sql text by argValue[]
        /// also called by CrudEdit.cs
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public string GetArgsText(List<object> args)
        {
            var sqlArgs = "";
            if (args != null && args.Count > 0)
            {
                for (var i = 0; i < args.Count; i += 2)
                    sqlArgs += (args[i + 1] == null ? "null" : args[i + 1].ToString()) + ",";

                sqlArgs = "(" + sqlArgs[0..^1] + ")";
            }
            return sqlArgs;
        }

        //set _now, consider time difference
        private void SetNow(){
            _now = DateTime.Now;
        }

        //check and log if slow query
        private async Task LogSlowSql(string sql)
        {
            if (_Fun.Config.SlowSql > 0)
            {
                var diff = (int)_Date.MiniSecDiff(_now, DateTime.Now);
                if (diff >= _Fun.Config.SlowSql)
                    await _Log.ErrorAsync($"Slow Sql({diff}): {sql}");
            }
        }        

        #region GetJson(s)
        public async Task<JObject> GetJsonAsync(string sql, List<object> sqlArgs = null)
        {
            var rows = await GetJsonsAsync(sql, sqlArgs);
            return (rows == null || rows.Count == 0) ? null : (JObject)rows[0];
        }

        public async Task<JArray> GetJsonsAsync(string sql, List<object> sqlArgs = null)
        {
            var reader = await GetReaderAsync(sql, sqlArgs);
            if (reader == null)
                return null;

            //read db rows into JArray
            var rows = new JArray();
            while (reader.Read())
                rows.Add(ReaderGetJson(reader));

            reader.Close();
            return (rows.Count == 0) ? null : rows;
        }
        #endregion

        #region GetInt(s)/GetStr(s)
        public async Task<int?> GetIntAsync(string sql, List<object> sqlArgs = null)
        {
            var list = await GetIntsAsync(sql, sqlArgs);
            return (list == null) ? (int?)null : list[0];
        }

        public async Task<List<int>> GetIntsAsync(string sql, List<object> sqlArgs = null)
        {
            var reader = await GetReaderForModelAsync(sql, sqlArgs);
            if (reader == null)
                return null;

            var list = new List<int>();
            while (reader.Read())
            {
                list.Add((int)reader[0]);
            }
            reader.Close();
            return (list.Count == 0) ? null : list;
        }

        /// <summary>
        /// get Db string column value, return null if not found !!
        /// string is nullable type !!
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlArgs"></param>
        /// <returns></returns>
        public async Task<string> GetStrAsync(string sql, List<object> sqlArgs = null)
        {
            var list = await GetStrsAsync(sql, sqlArgs);
            //return (list == null) ? null : list[0];
            return list[0];
        }

        public async Task<List<string>> GetStrsAsync(string sql, List<object> sqlArgs = null)
        {
            var reader = await GetReaderForModelAsync(sql, sqlArgs);
            if (reader == null)
                return null;

            var list = new List<string>();
            while (reader.Read())
                list.Add((string)reader[0]);

            reader.Close();
            return (list.Count == 0) ? null : list;
        }
        #endregion

        #region GetModel(s)      
        public async Task<T> GetModelAsync<T>(string sql, List<object> sqlArgs = null)
        {
            var rows = await GetModelsAsync<T>(sql, sqlArgs);
            return (rows == null || rows.Count == 0) ? default : rows[0];
        }
        
        public async Task<List<T>> GetModelsAsync<T>(string sql, List<object> sqlArgs = null)
        {
            var reader = await GetReaderForModelAsync(sql, sqlArgs);
            if (reader == null)
                return null;

            //get column name list
            var fname = new JObject();
            var fidLen = reader.FieldCount;
            for (var i = 0; i < fidLen; i++)
                fname[reader.GetName(i)] = i;

            //read rows
            var list = new List<T>();
            var status = true;
            var errorFid = "";      //error field id
            var props = Activator.CreateInstance<T>().GetType().GetProperties();
            try
            {
                while (reader.Read())
                {
                    var row = Activator.CreateInstance<T>();
                    foreach (var prop in props)
                    {
                        //if DB column name is in T & not null, then write this column into T
                        if (fname.Property(prop.Name) != null && !object.Equals(reader[prop.Name], DBNull.Value))
                        {
                            errorFid = prop.Name;   //now column name
                            prop.SetValue(row, reader[prop.Name], null);
                        }
                    }
                    list.Add(row);
                }
            }
            catch (Exception ex2)
            {
                //_Fun.DbStatus = false;
                status = false;
                var error = "Db.GetModelsAsync() error:(field=" + errorFid + ") " + GetSqlText(sql) + ", " + ex2.Message;
                await _Log.ErrorAsync(error);
            }

            reader.Close();
            return (status && list.Count > 0) ? list : null;
        }
        #endregion

        /// <summary>
        /// called outside
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public JObject ReaderGetJson(IDataReader reader)
        {
            //var dtFormat = _Fun.DbDtFormat;
            var dtFormat = _Fun.CsDtFmt;
            var row = new JObject();
            for (var i = 0; i < _colMap.Count; i++)
            {
                var fid = _colMap[i].Id;
                var type = _colMap[i].Num;
                row[fid] = reader.IsDBNull(i) ? "" :
                    //(type == DataTypeEnum.Datetime) ? reader.GetDateTime(i).ToString(dtFormat) :
                    (type == DataTypeEnum.Date) ? reader.GetDateTime(i).ToString(dtFormat) :
                    (type == DataTypeEnum.Bit) ? (reader.GetBoolean(i) ? 1 : 0) :
                    (type == DataTypeEnum.Int) ? Convert.ToInt32(reader[i]) :
                    reader[i].ToString();
            }
            return row;
        }
        
        //set _colMap, also call this method when initial reader
        private void SetColMap(IDataReader reader)
        {
            _colMap.Clear();    // = new ListJObject();  //reset
            //var keys = new List<string>();
            var colLen = reader.FieldCount;
            //string type;//, type2fid = "";
            int type;
            for (var i = 0; i < colLen; i++)
            {
                //fid = reader.GetName(i);
                var typeName = reader.GetDataTypeName(i).ToLower();
                //if (typeName.Contains("datetime"))
                //    type = DataTypeEnum.Datetime;
                if (typeName.Contains("date"))
                    type = DataTypeEnum.Date;
                else if (typeName.Contains("bit"))
                    type = DataTypeEnum.Bit;
                else if (typeName.Contains("int"))
                    type = DataTypeEnum.Int;
                else
                    type = DataTypeEnum.Other;

                _colMap.Add(new IdNumDto()
                {
                    Id = reader.GetName(i),
                    Num = type,
                });
                //keys.Add(fid);
            }
        }

        //get reader for get model()
        //return null when failed
        private async Task<IDataReader> GetReaderForModelAsync(string sql, List<object> sqlArgs = null)
        {
            //run reader
            if (!await InitCmdAsync(sql, sqlArgs))
                return null;

            try
            {
                SetNow();
                var reader = await _cmd.ExecuteReaderAsync();
                await LogSlowSql(sql);
                return reader;
            }
            catch (Exception ex)
            {
                await _Log.ErrorAsync($"_cmd.ExecuteReader() error: {GetSqlText(sql)}, {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// update db
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>affected rows count, -1 means error</returns>
        public async Task<int> ExecSqlAsync(string sql, List<object> sqlArgs = null)
        {
            if (!await InitCmdAsync(sql, sqlArgs))
                return 0;

            try
            {
                return _cmd.ExecuteNonQuery();
                //if (count == 0)
                //    return "Db.cs ExecSql() failed, change no row.(" + sql + ")";
            }
            catch (Exception ex)
            {
                await _Log.ErrorAsync($"Db.UpdateDb() error: {ex.Message}\nsql: {GetSqlText(sql)}");
                return 0;
            }
        }

        /// <summary>
        /// set row Status column, copy it when different rule !!
        /// </summary>
        /// <param name="table">table name, has dbo if need !!</param>
        /// <param name="kid">key field id</param>
        /// <param name="kvalue">client side key value, by sql args, avoid injection !!</param>
        /// <param name="status">true/false</param>
        /// <param name="statusId">status column id, default Status</param>
        /// <param name="where">other condition, default empty</param>
        /// <returns></returns>
        public async Task<bool> SetRowStatus(string table, string kid, object kvalue, 
            bool status, string statusId = "Status", string where = "")
        {
            var status2 = status ? 1 : 0;
            if (where != "")
                where = " and " + where;
            //var key2 = (kvalue.GetType() == typeof(string)) ? "'" + kvalue + "'" : kvalue.ToString();
            var sql = $"update {table} set {statusId}={status2} where {kid}=@{kid}{where};";
            return (await ExecSqlAsync(sql, new List<object>(){ kid, kvalue }) > 0);
        }

        #region transation (3 functions)
        //return error msg if any
        public async Task<bool> BeginTranAsync()
        {
            if (!await InitCmdAsync(null))
                return false;

            _tran = await _conn.BeginTransactionAsync();
            _cmd.Transaction = _tran;
            return true;
        }

        public async Task CommitAsync()
        {
            await _tran.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            await _tran.RollbackAsync();
        }
        #endregion

        #region cache remark
        /*
        private JArray GetJsonRowsByCache(DbReadModel sqlArg, CacheReadModel cacheModel)
        {
            //如果無法初始化 cache, 則直接讀取DB
            if (!InitCache())
                return GetJsonRowsByDb(sqlArg);

            //=== join table 的情形 ===
            //get sql hash
            JArray rows;
            if (sqlArg.ColList == "")
            {
                //sql 內容與前面相同
                //sqlArg.Sql = String.Format(_Fun2.CrudSql, sqlModel.Select, sqlModel.From, sqlModel.Where, sqlModel.Order);

                //如果找到cache資料則直接回傳
                var sqlHash = _cache.GetSqlHash(sqlArg);
                var str = _cache.GetQuery(sqlHash);
                if (str != _Fun.NullString)
                    return _Json.StrToArray(str);

                //以下讀取DB
                rows = GetJsonRowsByDb(sqlArg);

                //寫入cache(new thread)
                new Thread(delegate () { CacheSetQuery(_cacheModel, sqlHash, rows); }).Start();

                //回傳資料
                return rows;
            }

            //=== 不使用 join table 的情形 ===
            //改為選取所有欄位 !!
            var sql = sqlArg.Sql;
            sqlArg.Sql = String.Format(sqlArg.Sql, "*");
            EnumCacheStatus cacheStatus = EnumCacheStatus.Empty;
            rows = _cache.GetRows(_cache.GetSqlHash(sqlArg), _cacheModel, ref cacheStatus);

            //如果Ok(包含null), 則直接回傳
            if (cacheStatus == EnumCacheStatus.Ok || cacheStatus == EnumCacheStatus.Error)
                return rows;

            //以下需要更新 cache: 1.cache只有部分資料(partCache), 2.cache無資料(!partCache)
            //如果有遺漏的資料, 則查詢DB, 並且更新這個cache區塊資料

            //讀取DB
            if (!InitDb())
                return null;

            //case of 讀不到cache資料
            rows = GetJsonRowsByDb(sqlArg);
            if (rows == null || rows.Count == 0)
            {
                //記錄 cache的結果為null(new thread)
                if (_Fun.DbStatus)
                    new Thread(delegate () { CacheSetQuery(_cacheModel, _cache.GetSqlHash(sqlArg), null); }).Start();
                return null;
            }

            //case of 有cache資料
            //寫入 cache(new thread)
            new Thread(delegate () { CacheSetRows(_cacheModel, _cache.GetSqlHash(sqlArg), rows); }).Start();
            //set rows
            //rows = rows2;

            //只傳回所選取的欄位
            var cols = sqlArg.ColList.Replace(" ", "").Split(',');
            var newRows = new JArray();
            foreach (var row in rows)
            {
                var newRow = new JObject();
                foreach (var col in cols)
                    newRow[col] = row[col];
                newRows.Add(newRow);
            }
            return newRows;
        }

        //new thread
        private void CacheSetQuery(CacheReadModel cacheModel, string sqlHash, JArray rows)
        {
            if (!InitCache())
                return;

            _cache.SetQuery(cacheModel.Tables, sqlHash, (rows == null || rows.Count == 0) ? "" : _Json.ArrayToStr(rows));
            _cache.Dispose();
        }

        //new thread
        private void CacheSetRows(CacheReadModel cacheModel, string sqlHash, JArray rows)
        {
            if (!InitCache())
                return;

            //如果是更新整個查詢結果, 必須也寫入 Table, Column
            _cache.SetRows(_cacheModel, sqlHash, rows);
            _cache.Dispose();
        }
        */
        #endregion

        #region remark code
        /// <summary>
        /// insert identify, 考慮mssql, mysql, oracle, 所以從 updateDb() 獨立出來
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="argFids"></param>
        /// <param name="argValues"></param>
        /// <returns>identify值, 0表示失敗</returns>
        //public int InsertIdent(string sql, List<string> argFids = null, List<object> argValues = null)
        /*
        public bool Insert(DbModel sqlArg)
        {
            //sql += "; select @@getIdent;";
            if (!InitCmd(sqlArg))
                return false;

            try
            {
                //SetNow();
                SqlDataReader reader = _cmd.ExecuteReader();
                //CheckSlowAction(sqlArg.Sql);

                //int ident = 0;
                //if (reader.Read())
                //    ident = Convert.ToInt32(reader.GetValue(0));

                reader.Close();
                //return ident;
                return true;
            }
            catch (Exception ex)
            {
                _Log.LogError("Db.Insert() error: " + ex.Message + " \nsql:" + _cmd.CommandText);
                return false;
            }
        }
        */
        #endregion

    } //class
}
 
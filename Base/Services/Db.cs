using Base.Enums;
using Base.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Base.Services
{
    /// <summary>
    /// database read/write 
    /// </summary>
    public class Db : IDisposable
    {
        //OleDb <-> Sql, ex:OleDbConnection <-> SqlConnection
        //SqlConnection, SqlCommand, SqlTransaction
        private DbConnection _conn = null;   //or will compile error !!
        private DbCommand _cmd = null;
        private DbTransaction _tran;
        private IServiceProvider _di;       //DI

        //column mapping for update/insert, key-type-欄位序號
        private List<IdNumDto> _colMap = new List<IdNumDto>();

        //status
        private bool _status;    //db status

        //sql args pairs, ex:"UserId","001",,,
        private List<object> _sqlArgs;

        private DateTime _now;      //for sql slow action
        private string _dbStr;      //db connection string        
        private const int _dbSec = 600; //command second, can not too short

        /// <summary>
        /// constructor, new db, 
        /// note: if db server live but not access right, it still connect ok !!
        /// </summary>
        /// <param name="dbStr">db field name in config, default to "db"</param>
        /// <param name="di">for lightinject DI, for different database engine, use object type, or called must include LightInject !!</param>
        //public Db(string dbStr = "", object di = null)
        public Db(string dbStr = "")
        {
            //_dbStr: db field name at config, if length > 30, it will be connection string !!
            _dbStr = (dbStr == "") ? _Fun.Config.Db : dbStr;
            //_DI = (di == null) ? _Fun.GetDI() : (ServiceContainer)di;
            _di = _Fun.GetDiBox();

            //_userDataService = new UserDataService();
            //_userInfo = _Session.Read();

            //SetUserProfile();
            //_isTran = transaction;
            //InitDb(); //here !!
        }

        public DbConnection GetConnection()
        {
            return _conn;
        }

        /*
        public string GetUserId()
        {
            //return "001";
            return _Fun.GetBaseUser().UserId;
        }
        */

        /// <summary>
        /// initial db, when set command, it will initial Db auto
        /// if you need to operate reader(not by command), call this method by yourself !!
        /// </summary>
        /// <returns></returns>
        public bool InitDb()
        {
            if (_status)
                return true;

            //when _dbStr length > 30, treat it as connection string
            //var connStr = (_dbStr.Length > 30) ? _dbStr : _Config.GetDbStr(_dbStr);

            //_sqlDb = new T();
            //_conn = new IDbConnection(connStr);
            //_conn = _DI.GetService<string, DbConnection>(connStr);
            _conn = (DbConnection)_di.GetService(typeof(DbConnection));
            //set connect string will get error when connecting state
            if (_conn.ConnectionString == null || _conn.ConnectionString != _dbStr)
                _conn.ConnectionString = _dbStr;
            //_conn.State = ConnectionState.Connecting
            try
            {
                _conn.Open();
                _status = true;
            }
            catch (Exception ex)
            {
                //set instance variables
                _status = false;
                _conn = null;

                //not log db connection string for security reason
                _Log.Error("connect db error: " + ex.Message);
            }
            return _status;
        }

        public void Dispose()
        {
            //_cmd.Cancel();
            //_conn.Close();
            if (_cmd != null)
                _cmd.Dispose();
            if (_conn != null)
                _conn.Dispose();

            //dispose cache
            //if (_cache != null)
            //    _cache.Dispose();
        }

        /// <summary>
        /// check db connection is ready or not
        /// </summary>
        /// <returns></returns>
        public bool Status()
        {
            return _status;
        }

        /// <summary>
        /// initial command and add parameters, array: field name, value...
        /// OleDb does not support named parameters !!
        /// sqlClient ExecuteReader will call sp_executesql & reused execute plan, increase performance
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlArgs"></param>
        /// <param name="dbSec"></param>
        /// <returns></returns>
        private bool InitCmd(string sql, List<object> sqlArgs = null, int dbSec = _dbSec)
        {
            if (!InitDb())
                return false;

            //set instance variables
            _sqlArgs = sqlArgs;

            //init command
            if (_cmd == null)
            {
                //_cmd = new SqlCommand();
                _cmd = (DbCommand)_di.GetService(typeof(DbCommand));
                _cmd.Connection = _conn;
            }
            _cmd.CommandType = CommandType.Text;
            _cmd.CommandTimeout = dbSec;            
            _cmd.Parameters.Clear();    //must clear parameters first

            //set parameters
            //OleDb does not support named parameters !!
            if (sqlArgs != null && sqlArgs.Count > 0)
            {
                for (var i = 0; i < sqlArgs.Count; i = i + 2)
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
                    /*
                    //can not use iif here
                    if (argValues[i] == null)
                        _cmd.Parameters.AddWithValue("@" + sqlArg.ArgFids[i], DBNull.Value);
                    else
                        _cmd.Parameters.AddWithValue("@" + sqlArg.ArgFids[i], argValues[i].ToString());
                    */
                }
            }
            _cmd.CommandText = sql;

            //log debug info
            if (!string.IsNullOrEmpty(sql))
                _Log.Sql(GetSqlText(sql));
            return true;
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
                for (var i = 0; i < args.Count; i = i + 2)
                    sqlArgs += (args[i + 1] == null ? "null" : args[i + 1].ToString()) + ",";

                sqlArgs = "(" + sqlArgs.Substring(0, sqlArgs.Length - 1) + ")";
            }
            return sqlArgs;
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
                _cmd = (DbCommand)_di.GetService(typeof(DbCommand));
                _cmd.Connection = _conn;
                _cmd.CommandTimeout = _dbSec;
            }
            return _cmd;
        }        

        //set _now, consider time difference
        private void SetNow(){
            _now = DateTime.Now;
            //_userProfile.SetNow();
        }

        //check and log if slow query
        private void CheckSlowAction(string sql)
        {
            if (_Fun.Config.SlowSql > 0)
            {
                var diff = (int)_Date.MiniSecDiff(_now, DateTime.Now);
                if (diff >= _Fun.Config.SlowSql)
                    _Log.Error("Slow Sql(" + diff + "): " + sql);
            }
        }
        

        #region GetJson(s)
        public JObject GetJson(string sql, List<object> sqlArgs = null)
        {
            var rows = GetJsons(sql, sqlArgs);
            return (rows == null || rows.Count == 0) ? null : (JObject)rows[0];
        }

        public JArray GetJsons(string sql, List<object> sqlArgs = null)
        {
            var reader = GetReader(sql, sqlArgs);
            if (reader == null)
                return null;

            //read db rows into JArray
            var rows = new JArray();
            while (reader.Read())
                rows.Add(ReaderGetJson(reader));

            reader.Close();
            return (rows.Count == 0) ? null : rows;

            //return GetJsonsByDb(sqlArg);
            /*
            return (cacheModel == null || _Config.GetStr(_Fun.CacheHost, false) == "")
                ? GetJsonRowsByDb(sqlArg)
                : GetJsonRowsByCache(sqlArg, cacheModel);
                */
        }
        #endregion

        #region GetInt(s)/GetStr(s)
        public int? GetInt(string sql, List<object> sqlArgs = null)
        {
            var list = GetInts(sql, sqlArgs);
            return (list == null) ? (int?)null : list[0];
        }

        public List<int> GetInts(string sql, List<object> sqlArgs = null)
        {
            var reader = GetReaderForModel(sql, sqlArgs);
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
        public string GetStr(string sql, List<object> sqlArgs = null)
        {
            var list = GetStrs(sql, sqlArgs);
            return (list == null) ? null : list[0];
        }

        public List<string> GetStrs(string sql, List<object> sqlArgs = null)
        {
            var reader = GetReaderForModel(sql, sqlArgs);
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
        public T GetModel<T>(string sql, List<object> sqlArgs = null)
        {
            var rows = GetModels<T>(sql, sqlArgs);
            return (rows == null || rows.Count == 0) ? default(T) : rows[0];
        }
        
        public List<T> GetModels<T>(string sql, List<object> sqlArgs = null)
        {
            var reader = GetReaderForModel(sql, sqlArgs);
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
                _Log.Error("Db.GetModelRowsByDb() error:(field=" + errorFid + ") " + GetSqlText(sql) + ", " + ex2.Message);
            }

            reader.Close();
            return (status && list.Count > 0) ? list : null;
        }
        #endregion

        
        //when use bulk copy, need to handle reader directly !!
        public IDataReader GetReader(string sql, List<object> sqlArgs = null, int dbSec = _dbSec)
        {
            //init command
            if (!InitCmd(sql, sqlArgs, dbSec))
                return null;

            //read db
            //IDataReader reader = null;
            try
            {
                SetNow();
                var reader = _cmd.ExecuteReader();
                CheckSlowAction(sql);
                SetColMap(reader);
                return reader;
            }
            catch (Exception ex)
            {
                //_Fun.DbStatus = false;
                _Log.Error("_cmd.ExecuteReader() error: " + GetSqlText(sql) + ", " + ex.Message + "\n");
                return null;
            }
        }

        /*
        private string GetFrontDtFormat()
        {
            var baseU = _Fun.GetBaseUser();
            return (baseU == null)
                ? _Fun.Config.FrontDtFormat
                : baseU.FrontDtFormat;
        }
        */

        //called outside
        public JObject ReaderGetJson(IDataReader reader)
        {
            var dtFormat = _Fun.DbDtFormat;
            var row = new JObject();
            for (var i = 0; i < _colMap.Count; i++)
            {
                var fid = _colMap[i].Id;
                var type = _colMap[i].Num;
                row[fid] = reader.IsDBNull(i) ? "" :
                    (type == DataTypeEnum.Datetime) ? reader.GetDateTime(i).ToString(dtFormat) :
                    (type == DataTypeEnum.Date) ? reader.GetDateTime(i).ToString(dtFormat) :
                    (type == DataTypeEnum.Bit) ? (reader.GetBoolean(i) ? "1" : "0") :
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
                if (typeName.Contains("datetime"))
                    type = DataTypeEnum.Datetime;
                else if (typeName.Contains("date"))
                    type = DataTypeEnum.Date;
                else if (typeName.Contains("bit"))
                    type = DataTypeEnum.Bit;
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


        //get reader for get model()
        //return null when failed
        private IDataReader GetReaderForModel(string sql, List<object> sqlArgs = null)
        {
            //run reader
            if (!InitCmd(sql, sqlArgs))
                return null;

            try
            {
                SetNow();
                var reader = _cmd.ExecuteReader();
                CheckSlowAction(sql);
                return reader;
            }
            catch (Exception ex)
            {
                //_Fun.DbStatus = false;
                _Log.Error("_cmd.ExecuteReader() error: " + GetSqlText(sql) + ", " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// update db
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>affected rows count</returns>
        public int ExecSql(string sql, List<object> sqlArgs = null)
        {
            if (!InitCmd(sql, sqlArgs))
                return 0;

            try
            {
                //SetNow();
                //int rows = _cmd.ExecuteNonQuery();
                return _cmd.ExecuteNonQuery();
                //CheckSlowAction(sql);
                //return true;
            }
            catch (Exception ex)
            {
                //_Fun.DbStatus = false;
                _Log.Error("Db.UpdateDb() error: " + ex.Message + " \nsql:" + GetSqlText(sql));
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
        /// <returns>true/false</returns>
        public bool SetRowStatus(string table, string kid, object kvalue, bool status, string statusId = "Status", string where = "")
        {
            var status2 = status ? 1 : 0;
            if (where != "")
                where = " and " + where;
            //var key2 = (kvalue.GetType() == typeof(string)) ? "'" + kvalue + "'" : kvalue.ToString();
            var sql = string.Format("update {0} set {1}={2} where {3}=@{3}{4};", table, statusId, status2, kid, where);
            return ExecSql(sql, new List<object>(){ kid, kvalue }) > 0;
        }

        #region transation (3 functions)
        public void BeginTran()
        {
            InitCmd(null);
            _tran = _conn.BeginTransaction();
            _cmd.Transaction = _tran;
        }

        public void Commit()
        {
            _tran.Commit();
        }

        public void Rollback()
        {
            _tran.Rollback();
        }
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
 
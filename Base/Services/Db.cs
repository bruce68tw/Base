using Base.Enums;
using Base.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// 以 DI 提供的 DbConnection、DbCommand 執行資料存取，須先初始化 _Fun；使用後應非同步釋放。
    /// </summary>
    public class Db : IAsyncDisposable
    {
        private DbConnection? _conn;
        private DbCommand? _cmd;
        private DbTransaction? _tran;

        //column mapping for update/insert, key-type-欄位序號
        private List<IdNumDto> _colMaps = [];

        //status
        private bool _status;    //db status

        //sql args pairs, ex:"UserId","001",,,
        private List<object>? _sqlArgs;

        private DateTime _now;      //for sql slow action
        private readonly string _dbStr = "";      //db connection string        
        private const int _dbSec = 600; //command second, can not too short

        /// <summary>
        /// 保存連線字串但尚不開連線；空字串使用 _Fun.Config.Db。
        /// </summary>
        /// <param name="dbStr">資料庫連線字串，空字串時採用共用設定。</param>
        public Db(string dbStr = "")
        {
            //_dbStr: db field name at config, if length > 30, it will be connection string !!
            _dbStr = (dbStr == "") ? _Fun.Config.Db : dbStr;

            //_isTran = transaction;
            //InitDb(); //here !!
        }

        /// <summary>
        /// 取得目前連線，尚未初始化時為 null。
        /// </summary>
        public DbConnection? GetConnection()
        {
            return _conn;
        }

        /// <summary>
        /// 從 DI 取得連線並開啟；已就緒直接回傳 true，連線失敗記錄並回傳 false。
        /// </summary>
        public async Task<bool> InitDbA()
        {
            if (_status) return true;

            //when _dbStr length > 30, treat it as connection string
            //var connStr = (_dbStr.Length > 30) ? _dbStr : _Config.GetDbStr(_dbStr);

            _conn = (DbConnection)_Fun.DiBox!.GetService(typeof(DbConnection))!;
            //set connect string will get error when connecting state
            //if (_conn.ConnectionString == null || _conn.ConnectionString != _dbStr)
            //_conn.State = ConnectionState.Connecting
            try
            {
                _conn!.ConnectionString = _dbStr;
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
                await _Log.ErrorRootA("connect db error: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 非同步釋放命令及連線，不代表交易已提交。
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_cmd != null) await _cmd.DisposeAsync();
            if (_conn != null) await _conn.DisposeAsync();
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
        /// 初始化命令；參數為名稱、值交錯配對，名稱自動加 @，非日期與 null 的值會轉字串。
        /// 此具名參數方式不適用 OleDb。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlArgs"></param>
        /// <param name="dbSec"></param>
        /// <returns>初始化成功為 true，連線初始化失敗為 false。</returns>
        private async Task<bool> InitCmdA(string sql, List<object>? sqlArgs = null, int dbSec = _dbSec)
        {
            if (!await InitDbA()) return false;

            //set instance variables
            _sqlArgs = sqlArgs;

            //init command
            if (_cmd == null)
            {
                //_cmd = new SqlCommand();
                _cmd = (DbCommand)_Fun.DiBox!.GetService(typeof(DbCommand))!;
                _cmd!.Connection = _conn;
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
            if (sql != "") _Log.Sql(GetSqlText(sql!));
            return true;
        }


        /// <summary>
        /// 執行查詢並回傳讀取器，失敗可為 null；呼叫端須關閉讀取器，dbSec 單位秒。
        /// </summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="sqlArgs">依名稱和值交錯排列的 SQL 參數清單</param>
        /// <param name="dbSec">資料庫命令逾時秒數</param>
        public async Task<IDataReader?> GetReaderA(string sql, List<object>? sqlArgs = null, int dbSec = _dbSec)
        {
            //init command
            if (!await InitCmdA(sql, sqlArgs, dbSec)) return null;

            try
            {
                SetNow();
                var reader = await _cmd!.ExecuteReaderAsync();
                LogSlowSql(sql);
                SetColMap(reader);
                return reader;
            }
            catch (Exception ex)
            {
                await _Log.ErrorRootA($"_cmd.ExecuteReaderAsync() error: {GetSqlText(sql)}, {ex.Message}\n");
                return null;
            }
        }

        //get sql text by argValue[]
        private string GetSqlText(string sql)
        {
            return sql + (_sqlArgs == null ? "" : GetArgsText(_sqlArgs));
        }

        /// <summary>
        /// 將名稱、值交錯清單中的值串成記錄文字，不用於 SQL 參數綁定。
        /// </summary>
        /// <param name="args">供擷取參數值並寫入記錄的名稱和值交錯清單。</param>
        public string GetArgsText(List<object> args)
        {
            if (args.Count == 0) return "";

            var sqlArgs = "";
            for (var i = 0; i < args.Count; i += 2)
                sqlArgs += (args[i + 1] == null ? "null" : args[i + 1].ToString()) + ",";

            sqlArgs = "(" + sqlArgs[0..^1] + ")";
            return sqlArgs;
        }

        //set _now, consider time difference
        private void SetNow(){
            _now = DateTime.Now;
        }

        //check and log if slow query
        private void LogSlowSql(string sql)
        {
            if (_Fun.Config.SlowSql > 0)
            {
                var diff = (int)_Date.MiniSecDiff(_now, DateTime.Now);
                if (diff >= _Fun.Config.SlowSql)
                    _Log.Error($"Slow Sql({diff}): {sql}");
            }
        }        

        #region GetRow(s)
        /// <summary>
        /// 取得第一列 JObject；無資料或讀取失敗可回傳 null。
        /// </summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="sqlArgs">依名稱和值交錯排列的 SQL 參數清單</param>
        public async Task<JObject?> GetRowA(string sql, List<object>? sqlArgs = null)
        {
            var rows = await GetRowsA(sql, sqlArgs);
            return (rows == null || rows.Count == 0) 
                ? null : (JObject)rows[0];
        }

        /// <summary>
        /// 取得 JArray 並在正常讀取後關閉讀取器；無資料或捕捉到的錯誤回傳 null。
        /// </summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="sqlArgs">依名稱和值交錯排列的 SQL 參數清單</param>
        public async Task<JArray?> GetRowsA(string sql, List<object>? sqlArgs = null)
        {
            var reader = await GetReaderA(sql, sqlArgs);
            if (reader == null) return null;

            try
            {
                //read db rows into JArray
                var rows = new JArray();
                while (reader.Read())
                    rows.Add(GetRowByReader(reader));

                reader.Close();
                return (rows.Count == 0) ? null : rows;
            }
            catch (Exception ex)
            {
                await _Log.ErrorRootA($"Db.cs GetRowsA() failed: {ex.Message}");
                return null;
            }

        }
        #endregion

        #region GetInt(s)/GetStr(s)
        /// <summary>
        /// 取得第一列首欄的可空整數；無資料或讀取失敗可回傳 null。
        /// </summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="sqlArgs">依名稱和值交錯排列的 SQL 參數清單</param>
        public async Task<int?> GetIntA(string sql, List<object>? sqlArgs = null)
        {
            var list = await GetIntsA(sql, sqlArgs);
            return (list == null) ? null : list?[0];
        }

        /// <summary>
        /// 讀取首欄整數清單，DB NULL 保留為 null；無資料或查詢失敗回傳 null。
        /// </summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="sqlArgs">依名稱和值交錯排列的 SQL 參數清單</param>
        public async Task<List<int?>?> GetIntsA(string sql, List<object>? sqlArgs = null)
        {
            var reader = await GetReaderForModelA(sql, sqlArgs);
            if (reader == null) return null;

            //使用Convert.ToInt32可以轉換bit
            var list = new List<int?>();
            while (reader.Read())
                list.Add(reader.IsDBNull(0) ? null : Convert.ToInt32(reader[0]));

            reader.Close();
            return (list.Count == 0) ? null : list;
        }

        /// <summary>
        /// 取得第一列首欄字串；DB NULL 為空字串，無資料時可為 null。
        /// </summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="sqlArgs">依名稱和值交錯排列的 SQL 參數清單</param>
        public async Task<string?> GetStrA(string sql, List<object>? sqlArgs = null)
        {
            var list = await GetStrsA(sql, sqlArgs);
            return list?[0];
        }

        /// <summary>
        /// 讀取首欄字串清單，DB NULL 轉空字串；無資料時回傳 null。
        /// </summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="sqlArgs">依名稱和值交錯排列的 SQL 參數清單</param>
        public async Task<List<string>?> GetStrsA(string sql, List<object>? sqlArgs = null)
        {
            var reader = await GetReaderForModelA(sql, sqlArgs);
            if (reader == null) return null;

            var list = new List<string>();
            while (reader.Read())
                list.Add(reader.IsDBNull(0) ? "" : reader[0].ToString()!);

            reader.Close();
            return (list.Count == 0) ? null : list;
        }
        #endregion

        #region GetModel(s)      
        /// <summary>
        /// 將第一列映射為 IdStrDto，無資料或映射失敗回傳 null。
        /// </summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="sqlArgs">依名稱和值交錯排列的 SQL 參數清單</param>
        public async Task<IdStrDto?> GetIdStrA(string sql, List<object>? sqlArgs = null)
        {
            var rows = await GetModelsA<IdStrDto>(sql, sqlArgs);
            return (rows == null || rows.Count == 0) ? default : rows[0];
        }
        /// <summary>
        /// 將查詢結果映射為 IdStrDto 清單，無資料或映射失敗回傳 null。
        /// </summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="sqlArgs">依名稱和值交錯排列的 SQL 參數清單</param>
        public async Task<List<IdStrDto>?> GetIdStrsA(string sql, List<object>? sqlArgs = null)
        {
            return await GetModelsA<IdStrDto>(sql, sqlArgs);
        }
        /// <summary>
        /// 以同名公開屬性映射第一列；無資料或失敗回傳 default。
        /// </summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="sqlArgs">依名稱和值交錯排列的 SQL 參數清單</param>
        public async Task<T?> GetModelA<T>(string sql, List<object>? sqlArgs = null)
        {
            var rows = await GetModelsA<T>(sql, sqlArgs);
            return (rows == null || rows.Count == 0) ? default : rows[0];
        }
        
        /// <summary>
        /// 以同名公開屬性與資料庫原始型別映射新模型；需可無參數建構，無資料或映射失敗回傳 null。
        /// </summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="sqlArgs">依名稱和值交錯排列的 SQL 參數清單</param>
        public async Task<List<T>?> GetModelsA<T>(string sql, List<object>? sqlArgs = null)
        {
            var reader = await GetReaderForModelA(sql, sqlArgs);
            if (reader == null) return null;

            //get column name list
            var fname = new JObject();
            var fidLen = reader.FieldCount;
            for (var i = 0; i < fidLen; i++)
                fname[reader.GetName(i)] = i;

            //read rows
            var list = new List<T>();
            var status = true;
            var errorFid = "";      //error field id
            var props = Activator.CreateInstance<T>()!.GetType().GetProperties();
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
            catch (Exception ex)
            {
                //_Fun.DbStatus = false;
                status = false;
                var error = "Db.GetModelsA() error:(field=" + errorFid + ") " + GetSqlText(sql) + ", " + ex.Message;
                await _Log.ErrorRootA(error);
            }

            reader.Close();
            return (status && list.Count > 0) ? list : null;
        }
        #endregion

        /// <summary>
        /// 依先前欄位對照轉換目前列；DB NULL 轉空字串，日期轉共用格式。
        /// </summary>
        /// <param name="reader">目前位於待轉換資料列的資料讀取器</param>
        public JObject GetRowByReader(IDataReader reader)
        {
            //var dtFormat = _Fun.DbDtFormat;
            //var dtFormat = _Fun.CsDtFmt;
            var row = new JObject();
            for (var i = 0; i < _colMaps.Count; i++)
            {
                var fid = _colMaps[i].Id;
                var type = _colMaps[i].Num;
                row[fid] = reader.IsDBNull(i) ? "" :
                    (type == DbColTypeEnum.Datetime) ? reader.GetDateTime(i).ToString(_Fun.CsDtFmt) :
                    (type == DbColTypeEnum.Date) ? reader.GetDateTime(i).ToString(_Fun.CsDateFmt) :
                    (type == DbColTypeEnum.Bit) ? (reader.GetBoolean(i) ? 1 : 0) :
                    (type == DbColTypeEnum.Int) ? Convert.ToInt32(reader[i]) :
                    (type == DbColTypeEnum.Decimal) ? Convert.ToDecimal(reader[i]) :
                    reader[i].ToString();
            }
            return row;
        }
        
        //set _colMap, also call this method when initial reader
        private void SetColMap(IDataReader reader)
        {
            _colMaps.Clear();    // = new ListJObject();  //reset
            //var keys = new List<string>();
            var colLen = reader.FieldCount;
            //string type;//, type2fid = "";
            //int type;
            for (var i = 0; i < colLen; i++)
            {
                //fid = reader.GetName(i);
                var name = reader.GetDataTypeName(i).ToLower();
                int type = name.Contains("smalldatetime") ? DbColTypeEnum.Date :
                    name.Contains("datetime") ? DbColTypeEnum.Datetime :
                    name.Contains("date") ? DbColTypeEnum.Date :
                    name.Contains("bit") ? DbColTypeEnum.Bit :
                    name.Contains("int") ? DbColTypeEnum.Int :
                    name.Contains("decimal") ? DbColTypeEnum.Decimal :
                    DbColTypeEnum.Other;

                _colMaps.Add(new IdNumDto()
                {
                    Id = reader.GetName(i),
                    Num = type,
                });
                //keys.Add(fid);
            }
        }

        //get reader for get model()
        //return null when failed
        private async Task<IDataReader?> GetReaderForModelA(string sql, List<object>? sqlArgs = null)
        {
            //run reader
            if (!await InitCmdA(sql, sqlArgs)) return null;

            try
            {
                SetNow();
                var reader = await _cmd!.ExecuteReaderAsync();
                LogSlowSql(sql);
                return reader;
            }
            catch (Exception ex)
            {
                await _Log.ErrorRootA($"_cmd.ExecuteReaderAsync() error: {GetSqlText(sql)}, {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 執行異動並回傳筆數；初始化失敗為 0、一般 DB 錯誤為 -1、辨識出的唯一鍵錯誤為 -2。
        /// </summary>
        /// <param name="sql">要執行或解析的 SQL 字串</param>
        /// <param name="sqlArgs">依名稱和值交錯排列的 SQL 參數清單</param>
        public async Task<int> ExecSqlA(string sql, List<object>? sqlArgs = null)
        {
            if (!await InitCmdA(sql, sqlArgs)) return 0;

            try
            {
                return _cmd!.ExecuteNonQuery();
                //if (count == 0)
                //    return "Db.cs ExecSql() failed, change no row.(" + sql + ")";
            }
            catch (DbException ex)
            {
                //檢查是否唯一鍵重複
                int result = -1;
                try
                {
                    int errorNo = (ex as dynamic).Number;
                    if (_Fun.UniqueKeyErrorNo.Contains(errorNo))
                    {
                        result = -2;
                    }
                }
                catch
                {
                    result = -1;
                }

                await _Log.ErrorRootA($"Db.ExecSqlA() error: {ex.Message}\nsql: {GetSqlText(sql)}");
                return result;
            }
        }

        /// <summary>
        /// 依參數化主鍵更新狀態為 1 或 0，異動筆數大於 0 才回傳 true；結構片段直接組合。
        /// </summary>
        /// <param name="table">查詢或異動所用的資料表名稱，由可信設定提供</param>
        /// <param name="kid">主鍵欄位名稱，由可信設定提供</param>
        /// <param name="kvalue">要比對的主鍵值</param>
        /// <param name="status">寫入的狀態，true 對應 1，false 對應 0。</param>
        /// <param name="statusId">要更新的狀態欄位名稱</param>
        /// <param name="where">附加於更新語句的額外條件，由可信設定提供</param>
        public async Task<bool> SetRowStatus(string table, string kid, object kvalue, 
            bool status, string statusId = "Status", string where = "")
        {
            var status2 = status ? 1 : 0;
            if (where != "") where = " and " + where;
            //var key2 = (kvalue.GetType() == typeof(string)) ? "'" + kvalue + "'" : kvalue.ToString();
            var sql = $"update {table} set {statusId}={status2} where {kid}=@{kid}{where};";
            return (await ExecSqlA(sql, [kid, kvalue]) > 0);
        }

        #region transation (3 functions)
        //return error msg if any
        /// <summary>
        /// 在同一命令與連線啟動交易；初始化失敗回傳 false，交易錯誤可能拋出。
        /// </summary>
        public async Task<bool> BeginTranA()
        {
            if (!await InitCmdA("")) return false;

            _tran = await _conn!.BeginTransactionAsync();
            _cmd!.Transaction = _tran;
            return true;
        }

        /// <summary>
        /// 提交已啟動的交易；沒有交易或提交失敗會拋出例外。
        /// </summary>
        public async Task CommitA()
        {
            await _tran!.CommitAsync();
        }

        /// <summary>
        /// 回復已啟動的交易；沒有交易或回復失敗會拋出例外。
        /// </summary>
        public async Task RollbackA()
        {
            await _tran!.RollbackAsync();
        }
        #endregion

        #region cache remark
        /*
        private JArray GetRowsByCache(DbReadModel sqlArg, CacheReadModel cacheModel)
        {
            //如果無法初始化 cache, 則直接讀取DB
            if (!InitCache())
                return GetRowsByDb(sqlArg);

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
                rows = GetRowsByDb(sqlArg);

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
            rows = GetRowsByDb(sqlArg);
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

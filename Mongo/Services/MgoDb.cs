using Base.Services;
using Mongo.Enums;
using Mongo.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Mongo.Services
{
    /// <summary>
    /// 不同NoSql缺少一致性，所以這裡不繼承自定介面!! 
    /// MongoDB 基本 CRUD 輔助類別。
    /// 提供連線、建立、讀取、更新、刪除與資源釋放等基礎操作。
    /// </summary>
    public class MgoDb : IDisposable
    {
        //MongoClient 內建 connection pool, 依官方建議同一組連線字串應共用同一個 MongoClient 實例，
        //否則每次 new MgoDb() 都會各自建立一份連線池，反而造成連線數暴增、無法發揮 pool 的效果。
        private static readonly ConcurrentDictionary<string, MongoClient> _clientMap = new();

        private MongoClient? _client;   //有pool機制, 使用singleton(跨MgoDb實例共用, 見 s_clients)
        private IMongoDatabase? _db;
        private IMongoCollection<BsonDocument>? _collect;
        private IClientSessionHandle? _session;
        //private readonly string _dbStr = "";
        private string _table = "";
        private bool _isOk = false;

        public MgoDb(string dbStr = "")
        {
            if (dbStr == "") 
                dbStr = _Fun.Config.Db;

            try
            {
                var mongoUrl = new MongoUrl(dbStr);
                //if (string.IsNullOrWhiteSpace(mongoUrl.DatabaseName))
                //    throw new ArgumentException("Database name must be included in the connection string.", nameof(dbStr));

                //相同連線字串共用同一個 MongoClient(與其內部連線池), 避免重複建立連線池
                _client = _clientMap.GetOrAdd(dbStr, _ => new MongoClient(mongoUrl));
                _db = _client.GetDatabase(mongoUrl.DatabaseName);
                //_collection = _db.GetCollection<BsonDocument>(collectName);
                _isOk = true;
            }
            catch (Exception ex)
            {
                _Log.Error("MgoDb.cs Connect() failed: " + ex.Message);
                _isOk = false;
            }
            //return _isOk;
        }

        #region transation (3 functions)
        //return error msg if any
        public async Task<bool> BeginTranA()
        {
            if (!_isOk || _client == null || _db == null) return false;

            try
            {
                _session = await _client.StartSessionAsync();
                _session.StartTransaction();
                return true;
            }
            catch (Exception ex)
            {
                _session?.Dispose();
                _session = null;
                _Log.Error("MgoDb.cs BeginTranA() failed: " + ex.Message);
                return false;
            }
        }

        public async Task CommitA()
        {
            if (_session == null) return;

            try
            {
                await _session.CommitTransactionAsync();
            }
            finally
            {
                _session.Dispose();
                _session = null;
            }
        }

        public async Task RollbackA()
        {
            if (_session == null) return;

            try
            {
                await _session.AbortTransactionAsync();
            }
            finally
            {
                _session.Dispose();
                _session = null;
            }
        }
        #endregion

        /// <summary>
        /// 依 table 名稱取得(或重新取得)collection，table 改變時會重新指向新的 collection。
        /// </summary>
        public bool SetCollect(string table)
        {
            if (!_isOk) return false;

            if (table != _table)
            {
                _table = table;
                _collect = _db!.GetCollection<BsonDocument>(table);
            }
            return true;
        }

        /*
        public FilterDefinition<BsonDocument> CondToFilter(string cond = "")
        {
            return string.IsNullOrEmpty(cond)
                ? Builders<BsonDocument>.Filter.Empty : cond;
        }
        */

        public IMongoCollection<BsonDocument>? GetCollect()
        {
            return _collect;
        }

        /// <summary>
        /// 建立一筆文件到 MongoDB 集合中。
        /// </summary>
        /// <typeparam name="TDocument">文件型別</typeparam>
        /// <param name="row">要插入的文件</param>
        public bool Insert(string table, JObject row)
        {
            if (!SetCollect(table)) return false;

            /*
            IsConnected();
            if (docu == null)
                throw new ArgumentNullException(nameof(docu));
            */

            //var bson = row is BsonDocument docu2 ? docu2 : row.ToBsonDocument();
            try
            {
                if (_session == null)
                    _collect!.InsertOne(row.ToBsonDocument());
                else
                    _collect!.InsertOne(_session, row.ToBsonDocument());
                return true;
            }
            catch (Exception ex) {
                _Log.Error("MgoDb.cs Create() failed: " + ex.Message);
                return false;
            }
        }

        //計算查詢條件筆數
        public async Task<int> GetCountByFilterA(string table, FilterDefinition<BsonDocument> filter)
        {
            //if (!_isOk) return null;
            if (!SetCollect(table)) return default;

            //var filter = QitemsToFilter(qitems);
            var count = _session == null
                ? await _collect!.CountDocumentsAsync(filter)
                : await _collect!.CountDocumentsAsync(_session, filter);
            return (int)count;
        }

        /// <summary>
        /// 依照文件 ID 取得單一文件。
        /// </summary>
        /// <typeparam name="TDocument">文件型別</typeparam>
        /// <param name="rowId">MongoDB 文件 ID</param>
        /// <returns>找到的文件；否則回傳預設值</returns>
        public JObject? GetRowById(string table, string rowId)
        {
            if (string.IsNullOrWhiteSpace(rowId) || !SetCollect(table)) 
                return default;

            var filter = _MgoDb.IdToFilter(rowId);
            var row = (_session == null
                ? _collect!.Find(filter)
                : _collect!.Find(_session, filter)).FirstOrDefault();
            if (row == null) return default;

            //return BsonSerializer.Deserialize<JObject>(docu);
            return JObject.Parse(row.ToJson());
        }

        public async Task<JObject?> GetRowA(string table, List<MgoQitemDto>? qitems = null, string sorts = "")
        {
            var rows = await GetRowsA(table, qitems, sorts);
            return (rows == null)
                ? default
                : rows[0] as JObject;
        }

        /// <summary>
        /// 讀取集合中的文件列表，可加入過濾條件與筆數上限。
        /// </summary>
        /// <typeparam name="TDocument">回傳的文件型別</typeparam>
        /// <param name="filter">MongoDB 篩選條件，預設為全部文件</param>
        /// <param name="maxCount">最大回傳筆數，可為 null 表示不限制</param>
        /// <returns>符合條件的文件列表</returns>
        public async Task<JArray?> GetRowsA(string table, List<MgoQitemDto>? qitems = null, string sorts = "", int? maxCount = null)
        {
            var filter = _MgoDb.QitemsToFilter(qitems);
            return await GetRowsByFilterA(table, filter, sorts, maxCount);
        }

        public async Task<JArray?> GetRowsByFilterA(string table, FilterDefinition<BsonDocument> filter, string sorts = "", int? maxCount = null)
        {
            if (!SetCollect(table)) return default;

            //var filter = QitemsToFilter(qitems);
            var query = (_session == null)
                ? _collect!.Find(filter)
                : _collect!.Find(_session, filter);
            if (maxCount.HasValue)
                query = query.Limit(maxCount.Value);

            //加上排序
            QueryAddSort(query, sorts);

            var rows = await query.ToListAsync();
            return (rows == null)
                ? default
                : JArray.Parse(rows.ToJson());
        }

        public async Task<List<BsonDocument>?> GetBsonsA(string table, List<MgoQitemDto>? qitems = null, string sorts = "", int? maxCount = null)
        {
            var filter = _MgoDb.QitemsToFilter(qitems);
            return await GetBsonsByFilterA(table, filter, sorts, maxCount);
        }

        public async Task<List<BsonDocument>?> GetBsonsByFilterA(string table, FilterDefinition<BsonDocument> filter, string sorts = "", int? maxCount = null)
        {
            if (!SetCollect(table)) return default;

            //var filter = QitemsToFilter(qitems);
            var query = (_session == null)
                ? _collect!.Find(filter)
                : _collect!.Find(_session, filter);
            if (maxCount.HasValue)
                query = query.Limit(maxCount.Value);

            //加上排序
            QueryAddSort(query, sorts);

            return await query.ToListAsync();
        }

        /*
        /// <summary>
        /// 依照自訂條件取得單一文件。
        /// </summary>
        /// <typeparam name="TDocument">文件型別</typeparam>
        /// <param name="filter">MongoDB 篩選條件</param>
        /// <returns>找到的文件；否則回傳預設值</returns>
        public async Task<JArray?> GetRowsA(string table, string cond = "")
        {
            if (!SetCollect(table)) return default;

            var filter = CondToFilter(cond);
            var query = (_session == null)
                ? _collect!.Find(filter)
                : _collect!.Find(_session, filter);
            var rows = await query.ToListAsync();
            if (rows == null) return default;

            return JArray.Parse(rows.ToJson());
        }
        */

        /// <summary>
        /// mongodb 查詢條件加上排序
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sorts">多個排序欄位以逗號分隔, 字尾如果有":D" 表示降冪</param>
        private void QueryAddSort(IFindFluent<BsonDocument, BsonDocument> query, string sorts = "")
        {
            if (string.IsNullOrWhiteSpace(sorts)) return;

            // 假設多個排序條件用逗號隔開，例如: "CreateDate:D, Name"
            var sortList = sorts.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var sortBuilder = Builders<BsonDocument>.Sort;
            var definitions = new List<SortDefinition<BsonDocument>>();

            foreach (var item in sortList)
            {
                var parts = item.Split(':');
                var fieldName = parts[0].Trim();

                // 判斷是否有指定降冪 (":D")，否則預設為升冪 (Ascending)
                bool isDesc = (parts.Length > 1 && parts[1].Trim().Equals("D", StringComparison.OrdinalIgnoreCase));
                if (isDesc)
                    definitions.Add(sortBuilder.Descending(fieldName));
                else
                    definitions.Add(sortBuilder.Ascending(fieldName));
            }

            if (definitions.Count > 0)
            {
                // 組合多個排序欄位
                var combinedSort = sortBuilder.Combine(definitions);
                query.Sort(combinedSort);
            }
        }

        public async Task<T?> GetModelA<T>(string table, List<MgoQitemDto>? qitems = null, string sorts = "")
        {
            var rows = await GetModelsA<T>(table, qitems, sorts);
            return (rows == null || rows.Count == 0) ? default : rows[0];
        }

        public async Task<List<T>?> GetModelsA<T>(string table, List<MgoQitemDto>? qitems = null, 
            string sorts = "", int? maxCount = null)
        {
            if (!SetCollect(table)) return default;

            var errorFid = "";
            try
            {
                var filter = _MgoDb.QitemsToFilter(qitems);
                var query = _collect!.Find(filter);
                if (maxCount.HasValue)
                    query = query.Limit(maxCount.Value);

                QueryAddSort(query, sorts);
                var rows = await query.ToListAsync();
                if (rows.Count == 0) return null;

                var props = Activator.CreateInstance<T>()!.GetType().GetProperties();
                var result = new List<T>();
                foreach (var row in rows)
                {
                    var row2 = Activator.CreateInstance<T>();
                    var json = BsonSerializer.Deserialize<JObject>(row);
                    foreach (var prop in props)
                    {
                        if (json.TryGetValue(prop.Name, out var value) && value != null && value.Type != JTokenType.Null)
                        {
                            errorFid = prop.Name;
                            prop.SetValue(row2, value.ToObject(prop.PropertyType));
                        }
                    }
                    result.Add(row2);
                }

                return result;
            }
            catch (Exception ex)
            {
                _Log.Error("MgoDb.cs GetModelsA() error:(field=" + errorFid + "), " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 依照 ID 更新文件。
        /// </summary>
        /// <typeparam name="TDocument">文件型別</typeparam>
        /// <param name="rowId">MongoDB 文件 ID</param>
        /// <param name="row">更新後的文件內容</param>
        /// <returns>更新影響的筆數</returns>
        public bool Update(string table, string rowId, JObject row)
        {
            if (!SetCollect(table)) return false;

            //var cond = IdToCond(docuId).ToString();

            //if (!_isOk) return 0;
            //if (row == null)
            //    throw new ArgumentNullException(nameof(row));

            try
            {
                //只修改部分欄位使用 UpdateOne, 不是 ReplaceOne !!
                var filter = _MgoDb.IdToFilter(rowId);
                //var bson = row.ToBsonDocument();  //not work!!
                //var bson = _Bson.JsonToBson(row);
                //UpdateOne 頂層元素須為 $ 運算子, 直接傳入欄位文件會出現 "Element name 'xxx' is not valid"
                var bson = new BsonDocument("$set", _Bson.JsonToBson(row));   
                var result = _session == null
                    ? _collect!.UpdateOne(filter, bson)
                    : _collect!.UpdateOne(_session, filter, bson);
                //return result.ModifiedCount + result.MatchedCount;
                return true;
            }
            catch (Exception ex)
            {
                _Log.Error("MgoDb.cs Update() failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 依照 ID 刪除文件。
        /// </summary>
        /// <param name="rowId">MongoDB 文件 ID</param>
        /// <returns>刪除的筆數</returns>
        public bool Delete(string table, string rowId)
        {
            if (!SetCollect(table)) return false;

            //IsConnected();
            try
            {
                var filter = _MgoDb.IdToFilter(rowId);
                var result = _session == null
                    ? _collect!.DeleteOne(filter)
                    : _collect!.DeleteOne(_session, filter);
                return result.DeletedCount == 1;
            }
            catch (Exception ex)
            {
                _Log.Error("MgoDb.cs Delete() failed: " + ex.Message);
                return false;
            }
        }

        //刪除多筆id
        public async Task<int> DeleteRowsByIdsA(string table, List<string> keys)
        {
            if (!SetCollect(table)) return 0;

            //IsConnected();
            try
            {
                var filter = Builders<BsonDocument>.Filter.In("_id", keys);
                var result = _session == null
                    ? await _collect!.DeleteManyAsync(filter)
                    : await _collect!.DeleteManyAsync(_session, filter);
                return (int)result.DeletedCount;
            }
            catch (Exception ex)
            {
                _Log.Error("MgoDb.cs DeleteRowsA() failed: " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// 依照自訂條件刪除文件。
        /// </summary>
        /// <param name="filter">MongoDB 篩選條件</param>
        /// <returns>刪除的筆數</returns>
        public int DeleteRows(string table, List<MgoQitemDto> qitems)
        {
            if (qitems.Count == 0) return 0;
            if (!SetCollect(table)) return 0;

            //IsConnected();
            var filter = _MgoDb.QitemsToFilter(qitems);
            var result = _session == null
                ? _collect!.DeleteMany(filter)
                : _collect!.DeleteMany(_session, filter);
            return (int)result.DeletedCount;
        }

        /*
        public int DeleteByCond(string fid, string value)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(fid, value);
            return DeleteByFilter(filter);
        }
        */

        /// for API ??
        /// <summary>原子追加陣列欄位內容，並可同時套用其他更新。</summary>
        public bool PushToArray<TItem>(FilterDefinition<BsonDocument> filter, string field,
            IEnumerable<TItem> items, UpdateDefinition<BsonDocument>? additionalUpdate = null)
        {
            try
            {
                var documents = items.Select(item => item is BsonDocument document
                    ? document : item!.ToBsonDocument());
                var updates = new List<UpdateDefinition<BsonDocument>>
                {
                    Builders<BsonDocument>.Update.PushEach(field, documents)
                };
                if (additionalUpdate != null) updates.Add(additionalUpdate);

                var update = Builders<BsonDocument>.Update.Combine(updates);
                var result = _session == null
                    ? _collect!.UpdateOne(filter, update)
                    : _collect!.UpdateOne(_session, filter, update);
                return result.MatchedCount == 1;
            }
            catch (Exception ex)
            {
                _Log.Error("MgoDb.cs PushToArray() failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 釋放此實例對 MongoDB 的參考。_client 為跨實例共用(見 s_clients)，不會在此關閉連線池。
        /// </summary>
        public void Dispose()
        {
            _session?.Dispose();
            _session = null;
            _collect = null;
            _db = null;
            _client = null;
            //DbStr = string.Empty;
            //DbName = string.Empty;
            //CollectName = string.Empty;
        }

        /*
        /// <summary>
        /// 依照字串 ID 建立 MongoDB 篩選條件。
        /// </summary>
        /// <param name="docuId">文件 ID</param>
        /// <returns>MongoDB 篩選條件</returns>
        private JObject IdToCond(string docuId)
        {
            return new JObject { { "_id", docuId } };
        }
        */
    }
}

using Base.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Mongo
{
    /// <summary>
    /// 不同NoSql缺少一致性，所以這裡不繼承自定介面!! 
    /// MongoDB 基本 CRUD 輔助類別。
    /// 提供連線、建立、讀取、更新、刪除與資源釋放等基礎操作。
    /// </summary>
    public class MongoSvc : IDisposable
    {
        private MongoClient? _client;
        private IMongoDatabase? _db;
        private IMongoCollection<BsonDocument>? _collection;
        private bool _isOk = false;

        /// <summary>
        /// MongoDB 連線字串。
        /// </summary>
        //public string DbStr { get; private set; } = string.Empty;

        /// <summary>
        /// 目前使用的資料庫名稱。
        /// </summary>
        //public string DbName { get; private set; } = string.Empty;

        /// <summary>
        /// 目前使用的集合名稱。
        /// </summary>
        //public string CollectName { get; private set; } = string.Empty;

        /// <summary>
        /// 判斷是否已完成連線並可進行 MongoDB 操作。
        /// </summary>
        //public bool _IsConnected => _client != null && _db != null && _collection != null;

        /// <summary>
        /// 建立 MongoDB 連線。
        /// </summary>
        /// <param name="dbStr">MongoDB 連線字串</param>
        /// <param name="dbName">資料庫名稱</param>
        /// <param name="collectName">集合名稱</param>
        /*
        public void Connect(string connectStr, string dbName, string collectName)
        {
            // 連線字串不可空白
            if (string.IsNullOrWhiteSpace(connectStr))
                throw new ArgumentException("Connection string is required.", nameof(connectStr));

            // 資料庫名稱不可空白
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentException("Database name is required.", nameof(dbName));

            // 集合名稱不可空白
            if (string.IsNullOrWhiteSpace(collectName))
                throw new ArgumentException("Collection name is required.", nameof(collectName));

            _client = new MongoClient(connectStr);
            _db = _client.GetDatabase(dbName);
            _collection = _db.GetCollection<BsonDocument>(collectName);

            ConnectStr = connectStr;
            DbName = dbName;
            CollectName = collectName;
        }
        */

        public bool Connect(string dbStr, string collectName)
        {
            /*
            if (string.IsNullOrWhiteSpace(dbStr))
                throw new ArgumentException("Connection string is required.", nameof(dbStr));
            if (string.IsNullOrWhiteSpace(collectName))
                throw new ArgumentException("Collection name is required.", nameof(collectName));
            */
            try
            {
                var mongoUrl = new MongoUrl(dbStr);
                //if (string.IsNullOrWhiteSpace(mongoUrl.DatabaseName))
                //    throw new ArgumentException("Database name must be included in the connection string.", nameof(dbStr));

                _client = new MongoClient(mongoUrl);
                _db = _client.GetDatabase(mongoUrl.DatabaseName);
                _collection = _db.GetCollection<BsonDocument>(collectName);
                _isOk = true;
            }
            catch (Exception ex)
            {
                _Log.Error("MongoSvc.cs Connect() failed: " + ex.Message);
                _isOk = false;
            }
            return _isOk;

            //DbStr = dbStr;
            //DbName = mongoUrl.DatabaseName;
            //CollectName = collectName;
        }

        /*
        /// <summary>
        /// 確認目前已完成 MongoDB 連線，否則拋出例外。
        /// </summary>
        private void IsConnected()
        {
            var _isConnected = _client != null && _db != null && _collection != null;
            if (!_isConnected)
                throw new InvalidOperationException("MongoDB is not connected. Call Connect() first.");
        }
        */

        /// <summary>
        /// 建立一筆文件到 MongoDB 集合中。
        /// </summary>
        /// <typeparam name="TDocument">文件型別</typeparam>
        /// <param name="docu">要插入的文件</param>
        public bool Create<TDocument>(TDocument docu)
        {
            //if (!_isOk) return false;

            /*
            IsConnected();
            if (docu == null)
                throw new ArgumentNullException(nameof(docu));
            */

            var bson = docu is BsonDocument docu2 ? docu2 : docu.ToBsonDocument();
            try
            {
                _collection!.InsertOne(bson);
                return true;
            }
            catch (Exception ex) {
                _Log.Error("MgoCrudSvc.cs Create() failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 讀取集合中的文件列表，可加入過濾條件與筆數上限。
        /// </summary>
        /// <typeparam name="TDocument">回傳的文件型別</typeparam>
        /// <param name="filter">MongoDB 篩選條件，預設為全部文件</param>
        /// <param name="maxCount">最大回傳筆數，可為 null 表示不限制</param>
        /// <returns>符合條件的文件列表</returns>
        public List<TDocument>? Read<TDocument>(FilterDefinition<BsonDocument>? filter = null, int? maxCount = null)
        {
            //if (!_isOk) return null;

            var query = filter ?? Builders<BsonDocument>.Filter.Empty;
            var result = _collection!.Find(query);
            if (maxCount.HasValue)
                result = result.Limit(maxCount.Value);

            //var docus = result.ToList();
            return result
                .ToList()
                .Select(a => BsonSerializer.Deserialize<TDocument>(a))
                .ToList();
        }

        /// <summary>
        /// 依照文件 ID 取得單一文件。
        /// </summary>
        /// <typeparam name="TDocument">文件型別</typeparam>
        /// <param name="docuId">MongoDB 文件 ID</param>
        /// <returns>找到的文件；否則回傳預設值</returns>
        public TDocument? Get<TDocument>(string docuId)
        {
            //if (!_isOk) return default;
            if (string.IsNullOrWhiteSpace(docuId)) return default;

            var filter = IdToFilter(docuId);
            var docu = _collection!.Find(filter).FirstOrDefault();
            if (docu == null) return default;

            return BsonSerializer.Deserialize<TDocument>(docu);
        }

        /// <summary>
        /// 依照自訂條件取得單一文件。
        /// </summary>
        /// <typeparam name="TDocument">文件型別</typeparam>
        /// <param name="filter">MongoDB 篩選條件</param>
        /// <returns>找到的文件；否則回傳預設值</returns>
        public TDocument? Get<TDocument>(FilterDefinition<BsonDocument> filter)
        {
            //if (!_isOk) return default;

            var docu = _collection!.Find(filter).FirstOrDefault();
            if (docu == null) return default;

            return BsonSerializer.Deserialize<TDocument>(docu);
        }

        /// <summary>
        /// 依照 ID 更新文件。
        /// </summary>
        /// <typeparam name="TDocument">文件型別</typeparam>
        /// <param name="docuId">MongoDB 文件 ID</param>
        /// <param name="docu">更新後的文件內容</param>
        /// <returns>更新影響的筆數</returns>
        public bool Update<TDocument>(string docuId, TDocument docu)
        {
            return UpdateByFilter(IdToFilter(docuId), docu);
            /*
            //if (!_isOk) return false;
            if (docu == null)
                throw new ArgumentNullException(nameof(docu));

            try
            {
                var filter = IdToFilter(docuId);
                var bson = docu is BsonDocument docu2 ? docu2 : docu.ToBsonDocument();
                var result = _collection!.ReplaceOne(filter, bson);
                //return result.ModifiedCount + result.MatchedCount;
                return true;
            }
            catch (Exception ex)
            {
                _Log.Error("MgoCrudSvc.cs Update() failed: " + ex.Message);
                return false;
            }
            */
        }

        /// <summary>
        /// 依照自訂條件更新文件。
        /// </summary>
        /// <typeparam name="TDocument">文件型別</typeparam>
        /// <param name="filter">MongoDB 篩選條件</param>
        /// <param name="docu">更新後的文件內容</param>
        /// <returns>更新影響的筆數</returns>
        public bool UpdateByFilter<TDocument>(FilterDefinition<BsonDocument> filter, TDocument docu)
        {
            //if (!_isOk) return 0;
            if (docu == null)
                throw new ArgumentNullException(nameof(docu));

            try
            {
                var bson = docu is BsonDocument docu2 ? docu2 : docu.ToBsonDocument();
                var result = _collection!.ReplaceOne(filter, bson);
                //return result.ModifiedCount + result.MatchedCount;
                return true;
            }
            catch (Exception ex)
            {
                _Log.Error("MgoCrudSvc.cs Update() failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 依照 ID 刪除文件。
        /// </summary>
        /// <param name="docuId">MongoDB 文件 ID</param>
        /// <returns>刪除的筆數</returns>
        public bool Delete(string docuId)
        {
            //IsConnected();
            try
            {
                var filter = IdToFilter(docuId);
                var result = _collection!.DeleteOne(filter);
                return (result.DeletedCount == 1);
            }
            catch (Exception ex)
            {
                _Log.Error("MgoCrudSvc.cs Delete() failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 依照自訂條件刪除文件。
        /// </summary>
        /// <param name="filter">MongoDB 篩選條件</param>
        /// <returns>刪除的筆數</returns>
        public int DeleteByFilter(FilterDefinition<BsonDocument> filter)
        {
            //IsConnected();
            var result = _collection!.DeleteMany(filter);
            return (int)result.DeletedCount;
        }

        public int DeleteByCond(string fid, string value)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(fid, value);
            return DeleteByFilter(filter);
        }

        /// <summary>
        /// 釋放 MongoDB 連線資源。
        /// </summary>
        public void Dispose()
        {
            _collection = null;
            _db = null;
            _client = null;
            //DbStr = string.Empty;
            //DbName = string.Empty;
            //CollectName = string.Empty;
        }

        /// <summary>
        /// 依照字串 ID 建立 MongoDB 篩選條件。
        /// </summary>
        /// <param name="docuId">文件 ID</param>
        /// <returns>MongoDB 篩選條件</returns>
        private FilterDefinition<BsonDocument> IdToFilter(string docuId)
        {
            return ObjectId.TryParse(docuId, out var objectId)
                ? Builders<BsonDocument>.Filter.Eq("_id", objectId)
                : Builders<BsonDocument>.Filter.Eq("_id", docuId);
        }
    }
}

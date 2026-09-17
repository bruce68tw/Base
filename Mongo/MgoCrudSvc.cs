using Base.Enums;
using Base.Models;
using Base.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace Mongo
{
    /// <summary>
    /// 不同NoSql缺少一致性，所以這裡不繼承自定介面!! 
    /// MongoDB 基本 CRUD 輔助類別。
    /// 提供連線、建立、讀取、更新、刪除與資源釋放等基礎操作。
    /// </summary>
    public class MgoCrudSvc : IDisposable
    {
        private MongoClient? _client;
        private IMongoDatabase? _db;
        private IMongoCollection<BsonDocument>? _collection;
        private bool _isOk = false;

        /// <summary>
        /// get page rows for dataTables
        /// </summary>
        /// <param name="readDto"></param>
        /// <param name="dtDto"></param>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        public async Task<JObject?> GetPageA(ReadDto readDto, DtDto dtDto, string ctrl = "")
        {
            var easyDto = _Model.Copy<DtDto, EasyDtDto>(dtDto);
            if (string.IsNullOrEmpty(dtDto.sort) && dtDto.order != null && dtDto.order.Count > 0)
            {
                //A/D + fidNo(base 0) for jquery dataTables
                easyDto.sort = (dtDto.order![0].dir == OrderTypeEnum.Asc ? "A" : "D") +
                    dtDto.order[0].fid;
            }
            return await GetPageA(readDto, easyDto, ctrl);
        }

        /// <summary>
        /// 依照查詢條件取得 MongoDB 分頁資料。
        /// </summary>
        public async Task<JObject?> GetPageA(ReadDto readDto, EasyDtDto dtDto, string ctrl = "")
        {
            if (string.IsNullOrWhiteSpace(readDto.ReadSql) || _collection == null)
                return null;

            dtDto.length = Math.Max(0, dtDto.length);
            dtDto.start = Math.Max(0, dtDto.start);

            var filterDocu = string.IsNullOrWhiteSpace(dtDto.findJson)
                ? []
                : BsonDocument.Parse(dtDto.findJson);
            var filter = new BsonDocumentFilterDefinition<BsonDocument>(filterDocu);

            var rowCount = dtDto.recordsFiltered;
            if (rowCount < 0)
                rowCount = (int)await _collection.CountDocumentsAsync(filter);

            var find = _collection.Find(filter);
            if (!string.IsNullOrWhiteSpace(dtDto.sort) && dtDto.sort.Length > 1)
            {
                var sortField = dtDto.sort[1..];
                var sort = dtDto.sort[0] == 'D'
                    ? Builders<BsonDocument>.Sort.Descending(sortField)
                    : Builders<BsonDocument>.Sort.Ascending(sortField);
                find = find.Sort(sort);
            }

            var docus = await find
                .Skip(dtDto.start)
                .Limit(dtDto.length)
                .ToListAsync();
            var rows = new JArray();
            foreach (var docu in docus)
                rows.Add(JObject.Parse(docu.ToJson()));

            return JObject.FromObject(new
            {
                data = rows,
                recordsFiltered = rowCount,
            });
        }


    }
}

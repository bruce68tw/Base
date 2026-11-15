using Base.Enums;
using Base.Models;
using Base.Services;
using Mongo.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace Mongo.Services
{
    /// <summary>
    /// 不同NoSql缺少一致性，所以這裡不繼承自定介面!! 
    /// MongoDB 基本 CRUD 輔助類別。
    /// 提供連線、建立、讀取、更新、刪除與資源釋放等基礎操作。
    /// </summary>
    public class MgoReadSvc : IDisposable
    {
        private MgoDb _db = null!;

        public MgoReadSvc(string dbStr = "")
        {
            _db = new MgoDb(dbStr);
        }

        /// <summary>
        /// get page rows for dataTables
        /// </summary>
        /// <param name="readDto"></param>
        /// <param name="dtDto"></param>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        public async Task<JObject?> GetPageA(MgoReadDto readDto, DtDto dtDto, string ctrl = "")
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
        public async Task<JObject?> GetPageA(MgoReadDto readDto, EasyDtDto dtDto, string ctrl = "")
        {
            var table = readDto.Table;
            if (string.IsNullOrWhiteSpace(table)) return default;
            if (!_db.SetCollect(table)) return default;

            dtDto.length = Math.Max(0, dtDto.length);
            dtDto.start = Math.Max(0, dtDto.start);

            var filter = _MgoDb.JsonStrToFilter(dtDto.findJson);
            var rowCount = dtDto.recordsFiltered;
            if (rowCount < 0)
                rowCount = (int)await _db.GetCountByFilterA(table, filter);

            //var filter = _db.CondToFilter(filter);
            var query = _db.GetCollect()!.Find(filter);
            if (!string.IsNullOrWhiteSpace(dtDto.sort) && dtDto.sort.Length > 1)
            {
                var sortField = dtDto.sort[1..];
                var sort = dtDto.sort[0] == 'D'
                    ? Builders<BsonDocument>.Sort.Descending(sortField)
                    : Builders<BsonDocument>.Sort.Ascending(sortField);
                query = query.Sort(sort);
            }

            var docus = await query
                .Skip(dtDto.start)
                .Limit(dtDto.length)
                .ToListAsync();
            var rows = (docus == null)
                ? null : JArray.Parse(docus.ToJson());
            /*
            var rows = new JArray();
            foreach (var docu in docus)
                rows.Add(JObject.Parse(docu.ToJson()));
            */

            return JObject.FromObject(new
            {
                data = rows,
                recordsFiltered = rowCount,
            });
        }

        public void Dispose()
        {
            //_collection = null;
            _db.Dispose();
            _db = null!;
            //_client = null;
            //_table = "";
            //DbStr = string.Empty;
            //DbName = string.Empty;
            //CollectName = string.Empty;
        }

    }
}

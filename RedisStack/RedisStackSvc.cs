using BaseAI.Interfaces;
using BaseAI.Models;
using StackExchange.Redis;

namespace RedisStack
{
    /// <summary>Redis Stack 向量資料庫服務骨架，目前尚未提供可用的向量存取。</summary>
    public class RedisStackSvc : IEmbedDbSvc
    {
        private readonly IDatabase _db;

        /// <summary>從呼叫端提供的 Redis 連線取得資料庫物件。</summary>
        /// <param name="redis">呼叫端提供的 Redis 連線多工器</param>
        public RedisStackSvc(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        /// <summary>預留的初始化入口，現行方法未讀取輸入或設定服務。</summary>
        /// <param name="httpClient">預留的 HTTP 用戶端，現行初始化不會使用</param>
        /// <param name="embedDbStr">預留的向量資料庫連線設定</param>
        /// <param name="embedDbName">todo:待確認 預期代表資料庫名稱或集合名稱。現行方法未讀取。</param>
        public void Init(HttpClient httpClient, string embedDbStr, string embedDbName)
        {

        }

        /// <summary>預留的向量新增入口，現行方法未寫入資料。</summary>
        /// <param name="id">用於識別目標資料的 ID</param>
        /// <param name="text">要處理的文字</param>
        /// <param name="embedding">預留要寫入的嵌入向量</param>
        /// <param name="metadata">預留隨向量保存的附加資料</param>
        public async Task AddA(
            string id,
            string text,
            float[] embedding,
            Dictionary<string, object>? metadata = null)
        {
            // HSET
        }

        /// <summary>預留的向量讀取入口，現行方法擲出 NotImplementedException。</summary>
        /// <param name="id">用於識別目標資料的 ID</param>
        public Task<EmbedDocuDto?> GetA(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>預留的向量刪除入口，現行方法擲出 NotImplementedException。</summary>
        /// <param name="id">用於識別目標資料的 ID</param>
        public Task DeleteA(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>預留的資料清除入口，現行方法擲出 NotImplementedException。</summary>
        public Task ClearA()
        {
            throw new NotImplementedException();
        }

        //todo
        /// <summary>預留的向量檢索入口，現行方法回傳空字串。</summary>
        /// <param name="vector">預留作為相似度檢索輸入的向量</param>
        public async Task<string> VectorToTextA(float[] vector  /*, int topK = 5*/)
        {
            return "";
        }

    }
}

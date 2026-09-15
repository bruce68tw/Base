using BaseAI.Interfaces;
using BaseAI.Models;
using StackExchange.Redis;

namespace RedisStack
{
    public class RedisStackSvc : IEmbedDbSvc
    {
        private readonly IDatabase _db;

        public RedisStackSvc(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public void Init(HttpClient httpClient, string embedDbStr, string embedDbName)
        {

        }

        public async Task AddA(
            string id,
            string text,
            float[] embedding,
            Dictionary<string, object>? metadata = null)
        {
            // HSET
        }

        public Task<EmbedDocuDto?> GetA(string id)
        {
            throw new NotImplementedException();
        }

        public Task DeleteA(string id)
        {
            throw new NotImplementedException();
        }

        public Task ClearA()
        {
            throw new NotImplementedException();
        }

        //todo
        public async Task<string> VectorToTextA(float[] vector  /*, int topK = 5*/)
        {
            return "";
        }

    }
}

using BaseAI.Models;

namespace BaseAI.Interfaces
{
    public abstract class AbEmbedDbSvc(string embedDbStr, string embedTableName, int embedDim) :  IAsyncDisposable
    {
        protected HttpClient _httpClient = new();
        protected string _embedDbStr = embedDbStr;
        protected string _embedTableName = embedTableName;
        protected int _embedDim = embedDim;


        public abstract Task<bool> CreateA(string tableName, string id, float[] vector, string fileId);

        public abstract Task<EmbedDocuDto?> GetA(string id);

        public abstract Task DeleteA(string id);

        public abstract Task<bool> DeleteByCondA(string table, string fid, string value);

        public abstract Task<string> VectorToTextA(string tableName, float[] vector);

        public abstract Task ClearA();

        public async ValueTask DisposeAsync()
        {
            _httpClient.Dispose();
            _httpClient = null!;

            await ValueTask.CompletedTask;
            GC.SuppressFinalize(this);
        }

    }
}

namespace BaseAI.Interfaces
{
    //LLM
    public abstract class AbLlmSvc : IAsyncDisposable
    {
        protected HttpClient _httpClient = new();
        protected string _llmUrl;
        protected string _embedUrl;
        protected int _embedDim;

        public AbLlmSvc(string llmUrl, string embedUrl = "", int embedDim = 0) { 
            _httpClient = new HttpClient();
            _llmUrl = llmUrl;
            _embedUrl = embedUrl;
            _embedDim = embedDim;
        }

        public abstract Task<string> AskA(string prompt, string question);

        //文字轉嵌入向量
        public abstract Task<float[]?> TextToVectorA(string text);

        public virtual async ValueTask DisposeAsync()
        {
            _httpClient.Dispose();
            _httpClient = null!;

            await ValueTask.CompletedTask;
            GC.SuppressFinalize(this);
        }

    }
}

namespace BaseAI.Interfaces
{
    //LLM 包含 llm、文字轉嵌入向量功能
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

        /// <summary>將指定格式的語音資料轉錄為文字；無法辨識語音時回傳空字串。</summary>
        /// <param name="audioData">音訊檔案或原始音訊資料。</param>
        /// <param name="mimeType">音訊 MIME 類型，必須符合 provider 支援的格式。</param>
        /// <param name="ct">取消轉錄要求的權杖。</param>
        public abstract Task<string> SpeechToTextA(
            ReadOnlyMemory<byte> audioData, string mimeType, CancellationToken ct = default);

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

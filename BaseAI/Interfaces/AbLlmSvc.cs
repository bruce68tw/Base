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

        /// <summary>將完整音檔依 prompt 產生回應文字；輸出格式與無人聲約定由呼叫端決定，失敗拋例外。</summary>
        /// <param name="audioData">完整音檔；支援格式由 provider 決定。裸 PCM 請先用 _Voice.ToWavA 明示規格轉成 WAV。</param>
        /// <param name="mimeType">實際音檔 MIME，不可僅改標籤冒充 WAV。</param>
        /// <param name="prompt">回應任務與輸出指示；必填，可為空白。</param>
        /// <param name="keywords">辨識術語；必填，可為空清單。</param>
        /// <param name="ct">取消轉換、傳送與讀取回應。</param>
        public abstract Task<string> SttA(
            ReadOnlyMemory<byte> audioData, string mimeType, string prompt, List<string> keywords, CancellationToken ct = default);

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

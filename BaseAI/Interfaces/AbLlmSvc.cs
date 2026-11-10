namespace BaseAI.Interfaces
{
    //LLM 包含 llm、文字轉嵌入向量功能
    /// <summary>提供文字問答、音訊辨識與文字嵌入的抽象基底，並管理供衍生實作使用的 HTTP 用戶端。</summary>
    public abstract class AbLlmSvc : IAsyncDisposable
    {
        protected HttpClient _httpClient = new();
        protected string _llmUrl;
        protected string _embedUrl;
        protected int _embedDim;

        /// <summary>設定服務端點及預期向量維度。</summary>
        /// <param name="llmUrl">文字問答服務的端點。</param>
        /// <param name="embedUrl">文字嵌入服務的端點，可為空字串。</param>
        /// <param name="embedDim">向量預期維度，由衍生實作決定是否驗證。</param>
        public AbLlmSvc(string llmUrl, string embedUrl = "", int embedDim = 0) { 
            _httpClient = new HttpClient();
            _llmUrl = llmUrl;
            _embedUrl = embedUrl;
            _embedDim = embedDim;
        }

        /// <summary>以系統提示詞及使用者輸入向模型提出文字問答。</summary>
        /// <param name="prompt">系統提示詞，描述模型的任務及回覆要求。</param>
        /// <param name="question">要交給模型的使用者輸入。</param>
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
        /// <summary>將文字轉為嵌入向量，失敗及空值約定依衍生實作。</summary>
        /// <param name="text">要向量化的文字。</param>
        public abstract Task<float[]?> TextToVectorA(string text);

        /// <summary>釋放基底服務持有的 HTTP 用戶端。</summary>
        public virtual async ValueTask DisposeAsync()
        {
            _httpClient.Dispose();
            _httpClient = null!;

            await ValueTask.CompletedTask;
            GC.SuppressFinalize(this);
        }

    }
}

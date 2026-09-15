using Base.Services;
using BaseAI.Models;
using System.Net.WebSockets;

namespace BaseAI.Interfaces
{
    //Live LLM
    public abstract class AbLiveLlmSvc(string llmUrl) : IAsyncDisposable
    {
        protected HttpClient _httpClient = new();
        protected string _llmUrl = llmUrl;
        //protected string _embedUrl;

        /// <summary>Gemini Live API 使用的 WebSocket 連線。</summary>
        protected ClientWebSocket _socket = null!;

        /// <summary>負責 WebSocket JSON 收送與 frame 組合的共用服務。</summary>
        protected SocketSvc _socketSvc = null!;

        //bool IsOpen { get; }
        public bool IsOpen => _socketSvc.IsOpen == true;

        public abstract Task ConnectA(LiveLlmOptDto optDto, CancellationToken ct = default);

        public abstract Task SendAudioA(ReadOnlyMemory<byte> audioMem, CancellationToken ct = default);

        public abstract Task SendAudioTurnCompleteA(CancellationToken ct = default);

        public abstract Task SendTextA(string text, CancellationToken ct = default);

        public abstract Task SendToolRespA(IEnumerable<LiveLlmToolRespDto> responses, CancellationToken ct = default);

        public abstract IAsyncEnumerable<LiveLlmRespDto> ReceiveA(CancellationToken ct = default);

        public abstract Task CloseA(CancellationToken ct = default);

        /// <summary>將內部 payload 交由 WebSocket 服務序列化並送出。</summary>
        protected Task SendJsonA(object payload, CancellationToken ct = default)
        {
            return _socketSvc?.SendJsonA(payload, ct)
                ?? throw new InvalidOperationException("Gemini Live is not connected.");
        }

        public async ValueTask DisposeAsync()
        {
            _httpClient.Dispose();
            _socketSvc?.Dispose();
            _socket?.Dispose();
            _socketSvc = null!;
            _socket = null!;
            _httpClient = null!;

            await ValueTask.CompletedTask;
            GC.SuppressFinalize(this);
        }

    }
}

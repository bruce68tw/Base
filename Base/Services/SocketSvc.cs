using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// WebSocket 連線的共用收送服務。
    /// 此類別不擁有 WebSocket 的生命週期；呼叫端仍負責建立與釋放 WebSocket。
    /// </summary>
    public sealed class SocketSvc : IDisposable
    {
        /// <summary>目前綁定的 WebSocket 連線實例。</summary>
        private readonly WebSocket _socket;

        /// <summary>
        /// 送出訊息時的互斥鎖，避免同一條連線同時執行多個 SendAsync。
        /// </summary>
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        /// <summary>
        /// 建立 SocketSvc 並綁定指定的 WebSocket 連線。
        /// </summary>
        public SocketSvc(WebSocket socket)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
        }

        /// <summary>取得目前 WebSocket 的連線狀態。</summary>
        public WebSocketState State => _socket.State;

        /// <summary>判斷目前連線是否為可傳輸的 Open 狀態。</summary>
        public bool IsOpen => State == WebSocketState.Open;

        /// <summary>
        /// 將物件序列化為 JSON 後，以文字訊息送出。
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task SendJsonA(object payload, CancellationToken cancelToken = default)
        {
            if (!IsOpen) return;

            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));

            // 每條 WebSocket 同時間只允許一個 SendAsync。
            await _sendLock.WaitAsync(cancelToken);
            try
            {
                if (IsOpen)
                {
                    await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text,
                        endOfMessage: true, cancelToken);
                }
            }
            finally
            {
                _sendLock.Release();
            }
        }

        /// <summary>
        /// 接收一個完整文字訊息並自動合併多個 frame。
        /// 對方要求關閉連線時回傳 null。
        /// </summary>
        public async Task<string?> ReceiveTextA(int maxMsgBytes = 256 * 1024, CancellationToken cancelToken = default)
        {
            var buffer = new byte[8 * 1024];
            using var message = new MemoryStream();

            while (true)
            {
                var result = await _socket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), cancelToken);

                if (result.MessageType == WebSocketMessageType.Close)
                    return null;

                // Gemini Live API 會以 Binary frame 傳送 JSON 文字內容，
                // 因此 Text 與 Binary 都接受，payload 一律視為 UTF-8 bytes。
                if (result.MessageType != WebSocketMessageType.Text &&
                    result.MessageType != WebSocketMessageType.Binary)
                    throw new WebSocketException("Only text or binary WebSocket messages are supported.");

                message.Write(buffer, 0, result.Count);
                if (message.Length > maxMsgBytes)
                    throw new WebSocketException("WebSocket message exceeds the allowed size.");

                if (result.EndOfMessage)
                    return Encoding.UTF8.GetString(message.GetBuffer(), 0, (int)message.Length);
            }
        }

        /// <summary>
        /// 若連線仍為開啟狀態，則送出 Close frame 並嘗試正常關閉連線。
        /// </summary>
        public async Task CloseIfOpenA(WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
            string? description = null)
        {
            if (IsOpen)
            {
                await _socket.CloseAsync(status, description, CancellationToken.None);
            }
        }

        /// <summary>
        /// 釋放服務內部資源（目前僅釋放送訊息鎖）。
        /// </summary>
        public void Dispose()
        {
            _sendLock.Dispose();
        }
    }
}

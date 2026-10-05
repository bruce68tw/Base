using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Base.Services
{
    /// <summary>
    /// WebSocket 連線的共用收送及生命週期服務。
    /// 可綁定外部建立的 WebSocket，或自行建立並管理 ClientWebSocket。
    /// </summary>
    public sealed class WebSocketSvc : IDisposable
    {
        /// <summary>由此服務接受或建立，並由此服務負責釋放的 WebSocket。</summary>
        private WebSocket? _socket;

        /// <summary>
        /// 送出訊息時的互斥鎖，避免同一條連線同時執行多個 SendAsync。
        /// </summary>
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        /*
        /// <summary>建立尚未綁定 WebSocket 的服務。</summary>
        public WebSocketSvc()
        {
        }
        */

        /// <summary>
        /// 直接透過 HTTP context 接受 WebSocket upgrade，並由此服務管理連線。
        /// </summary>
        /// <param name="httpContext">目前的 ASP.NET Core HTTP context。</param>
        public async Task AcceptA(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);
            if (!httpContext.WebSockets.IsWebSocketRequest)
                throw new InvalidOperationException("The HTTP request is not a WebSocket request.");
            if (_socket != null)
                throw new InvalidOperationException("A WebSocket is already attached.");

            _socket = await httpContext.WebSockets.AcceptWebSocketAsync();
        }

        /// <summary>建立並連線至遠端 WebSocket；此服務負責釋放建立出的 ClientWebSocket。</summary>
        /// <param name="uri">遠端 WebSocket endpoint。</param>
        /// <param name="requestHeaders">連線時要設定的 HTTP request headers。</param>
        /// <param name="ct">取消連線操作的權杖。</param>
        public async Task ConnectRemoteA(Uri uri, 
            IEnumerable<KeyValuePair<string, string>>? requestHeaders = null,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(uri);

            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }

            var socket = new ClientWebSocket();
            try
            {
                if (requestHeaders != null)
                {
                    foreach (var header in requestHeaders)
                        socket.Options.SetRequestHeader(header.Key, header.Value);
                }

                await socket.ConnectAsync(uri, ct);
            }
            catch
            {
                socket.Dispose();
                throw;
            }

            _socket = socket;
        }

        /// <summary>取得目前 WebSocket 的連線狀態；尚未綁定連線時回傳 None。</summary>
        public WebSocketState State => _socket?.State ?? WebSocketState.None;

        /// <summary>判斷目前 WebSocket 是否處於可傳輸資料的開啟狀態。</summary>
        public bool IsOpen => State == WebSocketState.Open;

        /// <summary>
        /// client -> server
        /// 接收一個完整 WebSocket 訊息並合併其分段 frame，再以 UTF-8 解碼回傳。
        /// 接受 Text 與 Binary 訊息類型；對端要求關閉連線時回傳 <see langword="null"/>。
        /// </summary>
        /// <param name="maxMsgBytes">單一訊息允許的最大位元組數，超過時擲回 <see cref="WebSocketException"/>。</param>
        /// <param name="cancelToken">取消接收操作的權杖。</param>
        /// <returns>完整訊息的 UTF-8 文字；對端關閉連線時為 <see langword="null"/>。</returns>
        public async Task<string?> GetDataA(int maxMsgBytes = 256 * 1024, CancellationToken cancelToken = default)
        {
            var socket = _socket ?? throw new InvalidOperationException("WebSocket is not attached or connected.");
            var buffer = new byte[8 * 1024];
            using var message = new MemoryStream();

            while (true)
            {
                var result = await socket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), cancelToken);

                if (result.MessageType == WebSocketMessageType.Close)
                    return null;

                // 部分 provider 會以 Binary frame 傳送 JSON 文字內容，因此兩種 frame 都視為 UTF-8 payload。
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
        /// server -> client
        /// 將物件序列化為 JSON，並以 UTF-8 文字訊息送出。
        /// 同一連線上的傳送會序列化執行；連線未開啟時不執行傳送。
        /// </summary>
        /// <param name="payload">要序列化並傳送的物件。</param>
        /// <param name="cancelToken">取消等待傳送鎖或傳送操作的權杖。</param>
        public async Task SendDataA(object payload, CancellationToken cancelToken = default)
        {
            var socket = _socket;
            if (socket?.State != WebSocketState.Open) return;

            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));

            // 每條 WebSocket 同時間只允許一個 SendAsync。
            await _sendLock.WaitAsync(cancelToken);
            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text,
                        endOfMessage: true, cancelToken);
                }
            }
            finally
            {
                _sendLock.Release();
            }
        }

        /// <summary>
        /// 若連線仍為開啟狀態，則送出 Close frame 並嘗試關閉連線。
        /// 此方法不會釋放 WebSocket；連線的最終釋放仍由建立它的呼叫端負責。
        /// </summary>
        /// <param name="status">送出的 WebSocket 關閉狀態。</param>
        /// <param name="description">提供給對端的關閉原因。</param>
        public async Task CloseA(WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
            string? description = null)
        {
            var socket = _socket;
            if (socket?.State == WebSocketState.Open)
            {
                await socket.CloseAsync(status, description, CancellationToken.None);
            }
        }

        /// <summary>
        /// 釋放服務持有的 WebSocket 與內部傳送鎖。
        /// </summary>
        public void Dispose()
        {
            _sendLock.Dispose();
            _socket?.Dispose();
            _socket = null;
        }
    }
}

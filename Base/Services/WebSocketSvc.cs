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
    /// 管理單一伺服端或用戶端 WebSocket，提供完整訊息接收及序列化傳送；使用後需釋放。
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
        /// 接受 HTTP WebSocket 升級；請求不符或已有連線時拋出例外。
        /// </summary>
        public async Task AcceptA(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);
            if (!httpContext.WebSockets.IsWebSocketRequest)
                throw new InvalidOperationException("The HTTP request is not a WebSocket request.");
            if (_socket != null)
                throw new InvalidOperationException("A WebSocket is already attached.");

            _socket = await httpContext.WebSockets.AcceptWebSocketAsync();
        }

        /// <summary>
        /// 先釋放舊連線再連至遠端，可設定請求標頭與取消權杖；連線失敗會拋出。
        /// </summary>
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

        /// <summary>
        /// 目前連線狀態，沒有連線時為 None。
        /// </summary>
        public WebSocketState State => _socket?.State ?? WebSocketState.None;

        /// <summary>
        /// 判斷目前狀態是否為 Open。
        /// </summary>
        public bool IsOpen => State == WebSocketState.Open;

        /// <summary>
        /// 接收完整文字或二進位訊息並以 UTF-8 解碼；關閉回傳 null，超過位元組上限會拋出。
        /// </summary>
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
        /// 將物件序列化為 JSON 文字後以鎖序列化傳送；未開啟時略過。
        /// </summary>
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
        /// 僅在 Open 狀態執行正常關閉握手，不使用取消權杖。
        /// </summary>
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
        /// 釋放傳送鎖與 socket，並清除連線參考。
        /// </summary>
        public void Dispose()
        {
            _sendLock.Dispose();
            _socket?.Dispose();
            _socket = null;
        }
    }
}

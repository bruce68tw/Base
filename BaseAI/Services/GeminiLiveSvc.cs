using Base.Services;
using BaseAI.Interfaces;
using BaseAI.Models;
using BaseAI.Enums;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Net.WebSockets;

namespace BaseAI.Services
{
    /// <summary>
    /// Gemini Live API 的 WebSocket 即時語音服務。
    /// 負責將應用程式內部的即時 LLM DTO 轉換為 Gemini Live JSON 訊息，並將回應轉回 DTO。
    /// </summary>
    public sealed class GeminiLiveSvc(string llmUrl) : AbLiveLlmSvc(llmUrl)
    {
        /// <summary>Gemini API 金鑰。</summary>
        //private readonly string _apiKey;

        /// <summary>Gemini Live API 使用的 WebSocket 連線。</summary>
        //private ClientWebSocket? _socket;

        /// <summary>負責 WebSocket JSON 收送與 frame 組合的共用服務。</summary>
        //private SocketSvc? _socketSvc;

        /*
        /// <summary>
        /// 建立 Gemini Live 服務。
        /// </summary>
        public GeminiLiveSvc(HttpClient httpClient, string apiKey)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }
        */

        /// <summary>取得 Gemini Live WebSocket 是否仍處於開啟狀態。</summary>
        //public bool IsOpen => _socketSvc?.IsOpen == true;

        /// <summary>
        /// 建立 Gemini Live WebSocket，送出模型設定、系統提示詞與工具宣告，並等待 setupComplete。
        /// </summary>
        public override async Task ConnectA(LiveLlmOptDto optDto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(optDto.EndPoint))
                throw new ArgumentException("Gemini Live endpoint is required.", nameof(optDto));
            //if (string.IsNullOrWhiteSpace(_apiKey))
            //    throw new InvalidOperationException("Gemini API key is not configured.");

            _socket = new ClientWebSocket();
            await _socket.ConnectAsync(new Uri(_llmUrl), ct);
            _socketSvc = new SocketSvc(_socket);

            // setup 必須是連線建立後送出的第一個 Gemini Live 訊息。
            await SendJsonA(new
            {
                setup = new
                {
                    model = optDto.Model.StartsWith("models/", StringComparison.Ordinal)
                        ? optDto.Model : $"models/{optDto.Model}",
                    generationConfig = new
                    {
                        responseModalities = optDto.AudioResponse ? new[] { "AUDIO" } : new[] { "TEXT" },
                        speechConfig = new
                        {
                            voiceConfig = new { prebuiltVoiceConfig = new { voiceName = optDto.VoiceName } }
                        }
                    },
                    outputAudioTranscription = new { },
                    tools = optDto.Tools.Select(tool => new
                    {
                        functionDeclarations = new[]
                        {
                            new
                            {
                                name = tool.Name,
                                description = tool.Description,
                                parameters = tool.Parameters
                            }
                        }
                    }).ToArray(),
                    systemInstruction = new
                    {
                        parts = new[] { new { text = optDto.SystemPrompt } }
                    }
                }
            }, ct);

            var response = await ReceiveTextA(256 * 1024, ct);
            if (response is null)
                throw new InvalidOperationException("Gemini Live 在 setup 後關閉連線。");

            var json = JObject.Parse(response);
            if (json["error"] != null)
                throw new InvalidOperationException(json["error"]?["message"]?.ToString() ?? "Gemini Live setup 失敗。");
            if (json["setupComplete"] == null)
                throw new InvalidOperationException("Gemini Live 未回傳 setupComplete。");
        }

        /// <summary>
        /// 傳送 16 kHz PCM 音訊資料。音訊內容會先以 Base64 放入 realtimeInput.audio。
        /// </summary>
        public override Task SendAudioA(ReadOnlyMemory<byte> audioMem, CancellationToken ct = default)
        {
            return SendJsonA(new
            {
                realtimeInput = new
                {
                    audio = new
                    {
                        mimeType = "audio/pcm;rate=16000",
                        data = Convert.ToBase64String(audioMem.ToArray())
                    }
                }
            }, ct);
        }

        /// <summary>通知 Gemini Live 目前的音訊回合已結束。</summary>
        public override Task SendAudioTurnCompleteA(CancellationToken ct = default)
        {
            return SendJsonA(new
            {
                realtimeInput = new { audioStreamEnd = true }
            }, ct);
        }

        /// <summary>以已完成的使用者回合傳送文字訊息。</summary>
        public override async Task SendTextA(string text, CancellationToken ct = default)
        {
            await SendJsonA(new
            {
                clientContent = new
                {
                    turns = new[] { new { role = "user", parts = new[] { new { text } } } },
                    turnComplete = true
                }
            }, ct);
        }

        /// <summary>回傳 Gemini Live 先前要求執行的工具結果。</summary>
        public override Task SendToolRespA(IEnumerable<LiveLlmToolRespDto> respDtos, CancellationToken ct = default)
        {
            var functionResponses = respDtos.Select(response => new
            {
                id = response.Id,
                name = response.Name,
                response = response.Response
            }).ToArray();

            return SendJsonA(new
            {
                toolResponse = new { functionResponses }
            }, ct);
        }

        /// <summary>
        /// 持續接收 Gemini Live 訊息，並依訊息內容產生錯誤、工具呼叫、音訊、逐字稿或回合完成事件。
        /// </summary>
        public override async IAsyncEnumerable<LiveLlmRespDto> ReceiveA([EnumeratorCancellation] CancellationToken ct = default)
        {
            while (IsOpen)
            {
                var respText = await ReceiveTextA(256 * 1024, ct);
                if (respText is null)
                    yield break;

                JObject? respJson = null;
                try { respJson = JObject.Parse(respText); }
                catch { }

                if (respJson == null)
                {
                    // 保留接收迴圈，讓後續訊息仍可繼續處理。
                    yield return new LiveLlmRespDto
                    {
                        Type = LiveLlmRespTypeEnum.Error,
                        Text = "Gemini Live 回傳無法解析的訊息。"
                    };
                    continue;
                }

                if (respJson["error"] != null)
                {
                    yield return new LiveLlmRespDto
                    {
                        Type = LiveLlmRespTypeEnum.Error,
                        Text = respJson["error"]?["message"]?.ToString() ?? "Gemini Live 回傳錯誤。"
                    };
                    continue;
                }

                var funCalls = respJson["toolCall"]?["functionCalls"] as JArray;
                if (funCalls != null)
                {
                    // 工具呼叫需要由上層執行後，再透過 SendToolRespA 回傳結果。
                    yield return new LiveLlmRespDto
                    {
                        Type = LiveLlmRespTypeEnum.ToolCall,
                        ToolCalls = funCalls.Select(call => new LiveLlmToolCallDto
                        {
                            Id = call["id"]?.ToString() ?? "",
                            Name = call["name"]?.ToString() ?? "",
                            ArgumentsJson = call["args"]?.ToString(Newtonsoft.Json.Formatting.None) ?? "{}"
                        }).ToArray()
                    };
                    continue;
                }

                var content = respJson["serverContent"];
                var turnParts = content?["modelTurn"]?["parts"] as JArray;
                if (turnParts != null)
                {
                    // 一個 modelTurn 可能同時包含多個音訊片段，因此逐一產生事件。
                    foreach (var part in turnParts)
                    {
                        var inlineData = part["inlineData"];
                        var audioData = inlineData?["data"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(audioData))
                        {
                            yield return new LiveLlmRespDto
                            {
                                Type = LiveLlmRespTypeEnum.Audio,
                                Audio = Convert.FromBase64String(audioData),
                                MimeType = inlineData?["mimeType"]?.ToString()
                                    ?? "audio/pcm;rate=24000"
                            };
                        }
                    }
                }

                var tranScript = content?["outputTranscription"]?["text"]?.ToString();
                if (!string.IsNullOrWhiteSpace(tranScript))
                {
                    // outputAudioTranscription 會將模型語音轉成文字事件。
                    yield return new LiveLlmRespDto
                    {
                        Type = LiveLlmRespTypeEnum.OutputTranscript,
                        Text = tranScript
                    };
                }

                if (content?["turnComplete"]?.Value<bool>() == true)
                    yield return new LiveLlmRespDto { Type = LiveLlmRespTypeEnum.Completed };
            }
        }

        /// <summary>接收並組合一個完整的 Gemini Live JSON 訊息。</summary>
        private Task<string?> ReceiveTextA(int maxMsgBytes = 256 * 1024, CancellationToken ct = default)
        {
            return _socketSvc?.ReceiveTextA(maxMsgBytes, ct)
                ?? throw new InvalidOperationException("Gemini Live is not connected.");
        }

        /// <summary>以正常關閉狀態結束 Gemini Live 工作階段。</summary>
        public override async Task CloseA(CancellationToken ct = default)
        {
            if (_socketSvc != null)
                await _socketSvc.CloseIfOpenA(WebSocketCloseStatus.NormalClosure, "Gemini Live session ended");
        }

        /*
        /// <summary>釋放 WebSocket 與其共用收送服務。</summary>
        public ValueTask DisposeAsync()
        {
            _socketSvc?.Dispose();
            _socket?.Dispose();
            _socketSvc = null;
            _socket = null;
            return ValueTask.CompletedTask;
        }
        */

    }
}

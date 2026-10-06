using BaseAI.Interfaces;
using BaseAI.Models;
using BaseAI.Enums;
using Base.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Net.WebSockets;
using System.Web;

namespace BaseAI.Services
{
    /// <summary>
    /// Qwen Omni Realtime API 的 WebSocket 即時語音服務。
    /// 負責將共用 Live LLM DTO 轉換為 Qwen Realtime 事件，並將回應事件轉回 DTO。
    /// </summary>
    public sealed class QwenLiveSvc(string llmUrl, WebSocketSvc uiSocketSvc)
        : AbLiveLlmSvc(llmUrl, uiSocketSvc)
    {
        private const int MaxMessageBytes = 256 * 1024;

        /// <summary>
        /// 建立 Qwen Realtime WebSocket、送出 session 設定，並等待 session.updated。
        /// API key 可放在 endpoint 的 api_key/key query，或由 DASHSCOPE_API_KEY 環境變數提供。
        /// </summary>
        public override async Task ConnectLlmA(LlmOptDto optDto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(optDto);

            var endpoint = string.IsNullOrWhiteSpace(optDto.EndPoint) ? _llmUrl : optDto.EndPoint;
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri) ||
                (endpointUri.Scheme != "wss" && endpointUri.Scheme != "ws"))
            {
                throw new ArgumentException("Qwen Realtime endpoint must be a valid WebSocket URL.", nameof(optDto));
            }

            var query = HttpUtility.ParseQueryString(endpointUri.Query);
            var apiKey = TakeQueryValue(query, "api_key")
                ?? TakeQueryValue(query, "apikey")
                ?? TakeQueryValue(query, "key")
                ?? Environment.GetEnvironmentVariable("DASHSCOPE_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Qwen Realtime API key is required; configure api_key in the endpoint or DASHSCOPE_API_KEY.");

            var model = query["model"];
            if (string.IsNullOrWhiteSpace(model))
                model = optDto.Model;
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Qwen Realtime model is required.", nameof(optDto));
            query["model"] = model;

            var builder = new UriBuilder(endpointUri)
            {
                Query = BuildQuery(query)
            };

            await _llmSocketSvc.ConnectRemoteA(builder.Uri,
                [new KeyValuePair<string, string>("Authorization", $"Bearer {apiKey}")], ct);

            await WebToLlmDataA(new
            {
                type = "session.update",
                session = new
                {
                    modalities = optDto.AudioResponse ? new[] { "text", "audio" } : new[] { "text" },
                    instructions = optDto.SystemPrompt,
                    voice = string.IsNullOrWhiteSpace(optDto.VoiceName) ? "Cherry" : optDto.VoiceName,
                    input_audio_format = "pcm",
                    output_audio_format = "pcm",
                    turn_detection = (object?)null,
                    tools = optDto.Tools.Select(tool => new
                    {
                        type = "function",
                        name = tool.Name,
                        description = tool.Description,
                        parameters = tool.Parameters
                    }).ToArray(),
                    tool_choice = optDto.Tools.Count > 0 ? "auto" : "none"
                }
            }, ct);

            await WaitForSessionReadyA(ct);
            await SendHistoryA(optDto.History, ct);
        }

        /// <summary>傳送一段 16 kHz PCM 音訊。</summary>
        public override Task WebToLlmAudioA(ReadOnlyMemory<byte> audioMem, CancellationToken ct = default)
        {
            return WebToLlmDataA(new
            {
                type = "input_audio_buffer.append",
                audio = Convert.ToBase64String(audioMem.Span)
            }, ct);
        }

        /// <summary>提交目前音訊回合並要求 Qwen 產生回應。</summary>
        public override async Task WebToLlmAudioEndA(CancellationToken ct = default)
        {
            await WebToLlmDataA(new { type = "input_audio_buffer.commit" }, ct);
            await WebToLlmDataA(new { type = "response.create" }, ct);
        }

        /// <summary>以文字訊息建立使用者回合並要求 Qwen 產生回應。</summary>
        public override async Task WebToLlmTextA(string text, CancellationToken ct = default)
        {
            await WebToLlmDataA(new
            {
                type = "conversation.item.create",
                item = new
                {
                    type = "message",
                    role = "user",
                    content = new[] { new { type = "input_text", text } }
                }
            }, ct);
            await WebToLlmDataA(new { type = "response.create" }, ct);
        }

        /// <summary>回傳工具結果並要求 Qwen 繼續產生回應。</summary>
        public override async Task WebToLlmToolRespA(IEnumerable<LiveLlmToolRespDto> responses,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(responses);

            foreach (var response in responses)
            {
                await WebToLlmDataA(new
                {
                    type = "conversation.item.create",
                    item = new
                    {
                        type = "function_call_output",
                        call_id = response.Id,
                        output = Newtonsoft.Json.JsonConvert.SerializeObject(response.Response)
                    }
                }, ct);
            }

            await WebToLlmDataA(new { type = "response.create" }, ct);
        }

        /// <summary>接收 Qwen Realtime 事件並轉為共用 Live LLM 回應 DTO。</summary>
        public override async IAsyncEnumerable<LlmRespDto> OnLlmToWebTurnA(
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            while (IsOpen)
            {
                var message = await LlmToWebDataA(MaxMessageBytes, ct);
                if (message is null)
                    yield break;

                JObject? json;
                try { json = JObject.Parse(message); }
                catch { json = null; }

                if (json == null)
                {
                    yield return ErrorA("Qwen Realtime returned invalid JSON.");
                    continue;
                }

                var eventType = json["type"]?.Value<string>();
                switch (eventType)
                {
                    case "error":
                        yield return ErrorA(json["error"]?["message"]?.Value<string>()
                            ?? json["message"]?.Value<string>()
                            ?? "Qwen Realtime returned an error.");
                        break;

                    case "response.audio.delta":
                        var audio = json["delta"]?.Value<string>();
                        if (!string.IsNullOrWhiteSpace(audio))
                        {
                            yield return new LlmRespDto
                            {
                                Type = LlmRespTypeEnum.Audio,
                                Audio = Convert.FromBase64String(audio),
                                MimeType = "audio/pcm;rate=24000"
                            };
                        }
                        break;

                    case "conversation.item.input_audio_transcription.completed":
                        var inputText = json["transcript"]?.Value<string>();
                        if (!string.IsNullOrWhiteSpace(inputText))
                        {
                            yield return new LlmRespDto
                            {
                                Type = LlmRespTypeEnum.InputTranScript,
                                Text = inputText
                            };
                        }
                        break;

                    case "response.audio_transcript.delta":
                        var transcript = json["delta"]?.Value<string>();
                        if (!string.IsNullOrWhiteSpace(transcript))
                        {
                            yield return new LlmRespDto
                            {
                                Type = LlmRespTypeEnum.OutputTranScript,
                                Text = transcript
                            };
                        }
                        break;

                    case "response.text.delta":
                        var text = json["delta"]?.Value<string>();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            yield return new LlmRespDto
                            {
                                Type = LlmRespTypeEnum.Text,
                                Text = text
                            };
                        }
                        break;

                    case "response.function_call_arguments.done":
                        yield return new LlmRespDto
                        {
                            Type = LlmRespTypeEnum.ToolCall,
                            ToolCalls =
                            [
                                new LiveLlmToolCallDto
                                {
                                    Id = json["call_id"]?.Value<string>() ?? "",
                                    Name = json["name"]?.Value<string>() ?? "",
                                    ArgumentsJson = json["arguments"]?.Value<string>() ?? "{}"
                                }
                            ]
                        };
                        break;

                    case "response.done":
                        var totalTokens = json["response"]?["usage"]?["total_tokens"]?.Value<int>() ?? 0;
                        if (totalTokens > 0)
                            yield return new LlmRespDto { Type = LlmRespTypeEnum.Usage, TotalTokens = totalTokens };
                        yield return new LlmRespDto { Type = LlmRespTypeEnum.Completed };
                        break;
                }
            }
        }

        private async Task WaitForSessionReadyA(CancellationToken ct)
        {
            while (true)
            {
                var message = await LlmToWebDataA(MaxMessageBytes, ct);
                if (message is null)
                    throw new InvalidOperationException("Qwen Realtime closed the connection before session setup completed.");

                var json = JObject.Parse(message);
                var eventType = json["type"]?.Value<string>();
                if (eventType == "session.updated")
                    return;
                if (eventType == "error")
                    throw new InvalidOperationException(json["error"]?["message"]?.Value<string>()
                        ?? "Qwen Realtime session setup failed.");
            }
        }

        private async Task SendHistoryA(IReadOnlyCollection<LlmHistoryDto> history,
            CancellationToken ct)
        {
            foreach (var message in history)
            {
                if (string.IsNullOrWhiteSpace(message.Text) ||
                    (message.Role != "user" && message.Role != "assistant"))
                {
                    continue;
                }

                var isUser = message.Role == "user";
                await WebToLlmDataA(new
                {
                    type = "conversation.item.create",
                    item = new
                    {
                        type = "message",
                        role = isUser ? "user" : "assistant",
                        content = new[]
                        {
                            new { type = isUser ? "input_text" : "text", text = message.Text }
                        }
                    }
                }, ct);
            }
        }

        private static string? TakeQueryValue(NameValueCollection query, string key)
        {
            var value = query[key];
            query.Remove(key);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static string BuildQuery(NameValueCollection query)
        {
            return string.Join("&", query.AllKeys
                .Where(key => key != null)
                .Select(key => $"{Uri.EscapeDataString(key!)}={Uri.EscapeDataString(query[key!] ?? string.Empty)}"));
        }

        private static LlmRespDto ErrorA(string message)
        {
            return new LlmRespDto { Type = LlmRespTypeEnum.Error, Text = message };
        }

    }
}

using Base.Services;
using BaseAI.Enums;
using BaseAI.Interfaces;
using BaseAI.Models;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace BaseAI.Services
{
    /// <summary>
    /// Gemini Live API 的 WebSocket 即時語音服務。
    /// 負責將應用程式內部的即時 LLM DTO 轉換為 Gemini Live JSON 訊息，並將回應轉回 DTO。
    /// </summary>
    public sealed class GeminiLiveSvc(string llmUrl, WebSocketSvc uiSocketSvc)
        : AbLiveLlmSvc(llmUrl, uiSocketSvc)
    {

        /// <summary>
        /// 建立 Gemini Live WebSocket，送出模型設定、系統提示詞與工具宣告，並等待 setupComplete。
        /// </summary>
        public override async Task ConnectLlmA(LiveLlmOptDto optDto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(optDto.EndPoint))
                throw new ArgumentException("Gemini Live endpoint is required.", nameof(optDto));
            //if (string.IsNullOrWhiteSpace(_apiKey))
            //    throw new InvalidOperationException("Gemini API key is not configured.");

            // WebSocketSvc 負責建立及替換 provider 的上游 ClientWebSocket。
            await _llmSocketSvc.ConnectRemoteA(new Uri(_llmUrl), ct: ct);

            // setup 必須是連線建立後送出的第一個 Gemini Live 訊息。
            await WebToLlmDataA(new
            {
                setup = new
                {
                    // Gemini 需要 models/ 前綴，設定檔可省略。
                    model = optDto.Model.StartsWith("models/", StringComparison.Ordinal)
                        ? optDto.Model : $"models/{optDto.Model}",
                    generationConfig = new
                    {
                        // 原生音訊模型只支援 AUDIO；文字回應改由 outputAudioTranscription 取得。
                        responseModalities = optDto.AudioResponse ? new[] { "AUDIO" } : new[] { "TEXT" },
                        speechConfig = new
                        {
                            voiceConfig = new { prebuiltVoiceConfig = new { voiceName = optDto.VoiceName } }
                        }
                    },
                    inputAudioTranscription = new { },
                    outputAudioTranscription = new { },
                    // 每個工具各自包成一組 functionDeclarations。
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

            // 等待 Gemini 確認 setup，完成前不可傳送音訊或文字。
            var resp = await LlmToWebDataA(256 * 1024, ct);
            if (resp is null)
                throw new InvalidOperationException("Gemini Live 在 setup 後關閉連線。");

            var json = JObject.Parse(resp);
            if (json["error"] != null)
                throw new InvalidOperationException(json["error"]?["message"]?.ToString() ?? "Gemini Live setup 失敗。");
            if (json["setupComplete"] == null)
                throw new InvalidOperationException("Gemini Live 未回傳 setupComplete。");

            // 重連後用 clientContent 求回歷史對話，讓模型延續前文。
            if (optDto.History.Count > 0)
            {
                // Gemini 的角色名稱為 model，不是 assistant。
                var turns = optDto.History
                    .Where(message => message.Role is "user" or "assistant" && !string.IsNullOrWhiteSpace(message.Text))
                    .Select(message => new
                    {
                        role = message.Role == "assistant" ? "model" : "user",
                        parts = new[] { new { text = message.Text } }
                    })
                    .ToArray();

                if (turns.Length > 0)
                {
                    await WebToLlmDataA(new
                    {
                        clientContent = new
                        {
                            turns,
                            // false 表示只是載入背景，不要讓模型現在回覆。
                            turnComplete = false
                        }
                    }, ct);
                }
            }
        }

        /// <summary>
        /// 傳送 16 kHz PCM 音訊資料。音訊內容會先以 Base64 放入 realtimeInput.audio。
        /// </summary>
        public override Task WebToLlmAudioA(ReadOnlyMemory<byte> audioMem, CancellationToken ct = default)
        {
            return WebToLlmDataA(new
            {
                realtimeInput = new
                {
                    audio = new
                    {
                        mimeType = "audio/pcm;rate=16000",
                        data = Convert.ToBase64String(audioMem.Span)
                    }
                }
            }, ct);
        }

        /// <summary>通知 Gemini Live 目前的音訊回合已結束。</summary>
        public override Task WebToLlmAudioEndA(CancellationToken ct = default)
        {
            _Log.Info("WebToLlmAudioEndA");
            return WebToLlmDataA(new
            {
                realtimeInput = new { audioStreamEnd = true }
            }, ct);
        }

        /// <summary>以已完成的使用者回合傳送文字訊息。</summary>
        public override async Task WebToLlmTextA(string text, CancellationToken ct = default)
        {
            await WebToLlmDataA(new
            {
                clientContent = new
                {
                    turns = new[] { new { role = "user", parts = new[] { new { text } } } },
                    // true 表示使用者輸入完整，模型可立即回覆。
                    turnComplete = true
                }
            }, ct);
        }

        /// <summary>回傳 Gemini Live 先前要求執行的工具結果。</summary>
        public override Task WebToLlmToolRespA(IEnumerable<LiveLlmToolRespDto> respDtos, CancellationToken ct = default)
        {
            _Log.Info("WebToLlmToolRespA");
            // id 必須與 toolCall 的 id 相符，Gemini 才能對應結果。
            var functionResponses = respDtos.Select(response => new
            {
                id = response.Id,
                name = response.Name,
                response = response.Response
            }).ToArray();

            return WebToLlmDataA(new
            {
                toolResponse = new { functionResponses }
            }, ct);
        }

        /// <summary>
        /// 持續接收 Gemini Live 訊息，並依訊息內容產生錯誤、工具呼叫、音訊、逐字稿或回合完成事件。
        /// </summary>
        public override async IAsyncEnumerable<LiveLlmRespDto> LlmToWebBatchA([EnumeratorCancellation] CancellationToken ct = default)
        {
            _Log.Info("LlmToWebBatchA");
            while (IsOpen)
            {
                var respText = await LlmToWebDataA(256 * 1024, ct);
                if (respText is null)
                    yield break;   // 上游已關閉連線

                //_Log.Info("Gemini Live recv: " + (respText.Length > 300 ? respText[..300] : respText));

                // 訊息可能不是 JSON，解析失敗以 null 表示。
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
                    // provider 端錯誤只回報，不中斷接收。
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
                // 音訊在 modelTurn.parts[].inlineData，預設輸出為 24 kHz PCM。
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
                        Type = LiveLlmRespTypeEnum.OutputTranScript,
                        Text = tranScript
                    };
                }

                var inputTranScript = content?["inputTranscription"]?["text"]?.ToString();
                if (!string.IsNullOrWhiteSpace(inputTranScript))
                {
                    yield return new LiveLlmRespDto
                    {
                        Type = LiveLlmRespTypeEnum.InputTranScript,
                        Text = inputTranScript
                    };
                }

                var totalTokens = respJson["usageMetadata"]?["totalTokenCount"]?.Value<int>() ?? 0;
                if (totalTokens > 0)
                    yield return new LiveLlmRespDto { Type = LiveLlmRespTypeEnum.Usage, TotalTokens = totalTokens };

                // turnComplete 可能與音訊、逐字稿同封訊息出現，因此最後才產生。
                if (content?["turnComplete"]?.Value<bool>() == true)
                    yield return new LiveLlmRespDto { Type = LiveLlmRespTypeEnum.Completed };
            }
        }

    }
}

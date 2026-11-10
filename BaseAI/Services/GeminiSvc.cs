using Base.Services;
using BaseAI.Interfaces;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace BaseAI.Services
{
    /// <summary>透過 Gemini HTTP API 處理文字問答、音訊轉錄與文字嵌入。</summary>
    /// <param name="llmUrl">文字請求的 HTTP 端點。</param>
    /// <param name="embedUrl">傳給基底服務的文字嵌入端點，可為空字串。</param>
    /// <param name="embedDim">傳給基底服務的預期向量維度。</param>
    /// <param name="ffmpegPath">轉換壓縮音訊時使用的 FFmpeg 路徑或命令名稱。</param>
    public class GeminiSvc(string llmUrl, string embedUrl = "", int embedDim = 0, string ffmpegPath = "ffmpeg") :
        AbLlmSvc(llmUrl, embedUrl, embedDim)
    {
        private static readonly TimeSpan SttTimeout = TimeSpan.FromSeconds(20);

        private string Fail(string message)
        {
            _Log.Error("Gemini AskA: " + message);
            return "";
        }

        // 成功只回傳模型回答文字；失敗記錄錯誤並回傳空字串。
        /// <summary>送出 Gemini 文字問答請求並擷取回覆文字。</summary>
        /// <param name="prompt">傳給模型的系統提示詞。</param>
        /// <param name="question">傳給模型的使用者輸入。</param>
        public override async Task<string> AskA(string prompt, string question)
        {
            try
            {
                if (prompt == null || question == null)
                    return Fail("提示詞或問題不可為 null。");
                if (!Uri.TryCreate(_llmUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                    return Fail("服務端點必須為有效的 HTTPS 網址。");

                var json = new
                {
                    systemInstruction = new { parts = new[] { new { text = prompt } } },
                    contents = new[] { new { parts = new[] { new { text = question } } } }
                };
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var resp = await _httpClient.PostAsJsonAsync(uri, json, timeout.Token);
                if (!resp.IsSuccessStatusCode)
                    return Fail($"HTTP 請求失敗，狀態碼 {(int)resp.StatusCode}。");
                var respText = await resp.Content.ReadAsStringAsync(timeout.Token);
                var result = JObject.Parse(respText);
                if (result["error"] != null || result["promptFeedback"]?["blockReason"] != null)
                    return Fail("服務拒絕此請求。");
                var candidate = (result["candidates"] as JArray)?.First;
                var parts = candidate?["content"]?["parts"] as JArray;
                if (candidate?["finishReason"]?.Value<string>() != "STOP" || parts == null ||
                    !parts.Any(part => part["thought"]?.Value<bool>() != true &&
                        part["text"]?.Type == JTokenType.String && !string.IsNullOrWhiteSpace(part["text"]!.Value<string>())))
                    return Fail("未取得完整的回答文字。");
                // 回答可能分成多個片段；排除 thought，只依原順序串接文字。
                // JSON 陣列、物件或純文字的解讀與驗證由呼叫端負責。
                return string.Concat(parts
                    .Where(part => part["thought"]?.Value<bool>() != true &&
                        part["text"]?.Type == JTokenType.String)
                    .Select(part => part["text"]!.Value<string>()));
            }
            catch (Exception ex)
            {
                return Fail("GeminiSvc.AskA failed: " + ex.Message);
            }

            /*
            catch (OperationCanceledException)
            {
                return Fail("請求逾時或已取消。");
            }
            catch (JsonException)
            {
                return Fail("服務回應不是有效的 JSON。");
            }
            catch (Exception ex)
            {
                // 僅記錄例外類型，不輸出可能含金鑰網址、問題或服務回應的例外本文。
                return Fail($"請求失敗（{ex.GetType().Name}）。");
            }
            */
        }

        /// <summary>先轉換音訊為 WAV，再依提示詞要求模型產生文字。</summary>
        /// <param name="audioData">要轉換並處理的完整音檔。</param>
        /// <param name="mimeType">與音訊內容相符的 MIME 類型。</param>
        /// <param name="prompt">供模型處理音訊的任務或背景提示，可為空白。</param>
        /// <param name="keywords">供模型辨識的術語清單，可為空清單。</param>
        /// <param name="ct">取消音訊處理或 HTTP 請求的權杖。</param>
        public override async Task<string> SttA(
            ReadOnlyMemory<byte> audioData, string mimeType, string prompt, List<string> keywords, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(prompt);
            ArgumentNullException.ThrowIfNull(keywords);
            if (!Uri.TryCreate(_llmUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                throw new InvalidOperationException("Gemini STT endpoint must be an absolute HTTPS URL.");

            // 共用 20 秒上限涵蓋轉換、HTTP 傳送及讀取；不在抽象類別放轉換實作。
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(SttTimeout);
            var wavAudio = await _Voice.ToWavA(audioData, mimeType, ffmpegPath: ffmpegPath, ct: timeoutCts.Token);
            timeoutCts.Token.ThrowIfCancellationRequested();
            if (wavAudio == null)
                throw new InvalidOperationException("Gemini STT audio conversion failed; see _Voice.ToWavA error log.");

            var hints = JsonConvert.SerializeObject(new { keywords });
            var body = new {
                systemInstruction = new { parts = new[] { new { text = prompt } } },
                contents = new[] { new { role = "user", parts = new object[] {
                    new { text = "辨識詞彙提示（僅供詞彙參考，輸出格式依 systemInstruction）：" + hints },
                    new { inlineData = new { mimeType = "audio/wav", data = Convert.ToBase64String(wavAudio) } }
                } } }
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, _llmUrl) { Content = JsonContent.Create(body) };
            try
            {
                using var resp = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token);
                if (!resp.IsSuccessStatusCode)
                    // 不讀取或記錄供應商錯誤本文，避免洩漏音訊、網址與憑證。
                    throw new HttpRequestException("Gemini STT request failed.", null, resp.StatusCode);

                JObject json;
                try
                {
                    var text = await resp.Content.ReadAsStringAsync(timeoutCts.Token);
                    json = JObject.Parse(text);
                }
                catch (JsonException)
                {
                    throw new InvalidOperationException("Gemini STT returned an invalid response.");
                }

                if (json["promptFeedback"]?["blockReason"] != null)
                    throw new InvalidOperationException("Gemini STT request was blocked.");
                var candidate = (json["candidates"] as JArray)?.First;
                if (candidate == null || candidate["finishReason"]?.Value<string>() != "STOP")
                    throw new InvalidOperationException("Gemini STT did not return a completed transcript.");
                var parts = candidate["content"]?["parts"] as JArray;
                var textParts = parts?.Where(p => p["thought"]?.Value<bool>() != true)
                    .Select(p => p["text"]).Where(p => p?.Type == JTokenType.String).ToArray();
                if (textParts == null || textParts.Length == 0)
                    throw new InvalidOperationException("Gemini STT returned no response text.");
                // 共用層只抽取文字，不 trim、不解讀 [SILENCE]、不解析 JSON。
                return string.Concat(textParts.Select(p => p!.Value<string>()));
            }
            catch (HttpRequestException ex)
            {
                // 傳輸例外可能帶有含敏感查詢參數的 URL，不沿用訊息與 InnerException。
                throw new HttpRequestException("Gemini STT request failed.", null, ex.StatusCode);
            }
        }

        /// <summary>透過嵌入端點將文字轉為向量，未設定端點時回傳 null。</summary>
        /// <param name="text">要向量化的輸入文字。</param>
        public override async Task<float[]?> TextToVectorA(string text)
        {
            if (string.IsNullOrEmpty(_embedUrl)) return null;

            //var url = $"https://generativelanguage.googleapis.com/v1beta/models/text-embedding-004:embedContent?key={apiKey}";
            //var url = String.Format(_Xp.MyConfig.LlmEmbedUrl, apiKey);
            var json = new
            {
                taskType = "RETRIEVAL_QUERY",
                content = new { parts = new[] { new { text } } },
                outputDimensionality = _embedDim
            };

            //var json2 = JObject.FromObject(json).ToString(Newtonsoft.Json.Formatting.None);
            //using var content = new StringContent(json2, Encoding.UTF8, "application/json");

            var resp = await _httpClient.PostAsJsonAsync(_embedUrl, json);
            if (!resp.IsSuccessStatusCode)
            {
                _Log.Error($"Gemini Embedding API failed: {resp.StatusCode}");
                return null;
            }

            var respJson = JObject.Parse(await resp.Content.ReadAsStringAsync());
            var values = respJson["embedding"]?["values"] as JArray;
            return values?.ToObject<float[]>();
        }
    }
}

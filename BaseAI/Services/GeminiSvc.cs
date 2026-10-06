using Base.Services;
using BaseAI.Interfaces;
using Newtonsoft.Json.Linq;
using System.Buffers.Binary;
using System.Net.Http.Json;

namespace BaseAI.Services
{
    public class GeminiSvc(string llmUrl, string embedUrl = "", int embedDim = 0) : 
        AbLlmSvc(llmUrl, embedUrl, embedDim)
    {
        private static readonly TimeSpan SpeechToTextTimeout = TimeSpan.FromSeconds(20);

        private static byte[] AddPcmWavHeader(ReadOnlySpan<byte> pcmData)
        {
            var wavData = new byte[44 + pcmData.Length];
            "RIFF"u8.CopyTo(wavData.AsSpan(0, 4));
            BinaryPrimitives.WriteUInt32LittleEndian(wavData.AsSpan(4, 4), (uint)(36 + pcmData.Length));
            "WAVEfmt "u8.CopyTo(wavData.AsSpan(8, 8));
            BinaryPrimitives.WriteUInt32LittleEndian(wavData.AsSpan(16, 4), 16);
            BinaryPrimitives.WriteUInt16LittleEndian(wavData.AsSpan(20, 2), 1);
            BinaryPrimitives.WriteUInt16LittleEndian(wavData.AsSpan(22, 2), 1);
            BinaryPrimitives.WriteUInt32LittleEndian(wavData.AsSpan(24, 4), 16000);
            BinaryPrimitives.WriteUInt32LittleEndian(wavData.AsSpan(28, 4), 32000);
            BinaryPrimitives.WriteUInt16LittleEndian(wavData.AsSpan(32, 2), 2);
            BinaryPrimitives.WriteUInt16LittleEndian(wavData.AsSpan(34, 2), 16);
            "data"u8.CopyTo(wavData.AsSpan(36, 4));
            BinaryPrimitives.WriteUInt32LittleEndian(wavData.AsSpan(40, 4), (uint)pcmData.Length);
            pcmData.CopyTo(wavData.AsSpan(44));
            return wavData;
        }

        public override async Task<string> AskA(string prompt, string question)
        {
            if (string.IsNullOrEmpty(_llmUrl)) return "";

            //_httpClient.DefaultRequestHeaders.Clear();
            //_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var json = new
            {
                // 這裡設定系統提示詞
                systemInstruction = new{parts = new[]{new { text = prompt }}},
                // 這裡設定使用者的對話內容
                contents = new[]{new{parts = new[]{new { text = question }}}}
            };

            // 3. 序列化並發送請求
            //var bodyText = JsonSerializer.Serialize(json);
            //var content = new StringContent(bodyText, Encoding.UTF8, "application/json");
            //var url = $"{UrlApi}/{ModelType}:generateContent?key={_apiKey}";
            //var url = $"{UrlApi}/{ModelType}:generateContent?key={_apiKey}";

            try
            {
                var resp = await _httpClient.PostAsJsonAsync(_llmUrl, json);
                //resp.EnsureSuccessStatusCode();
                var respText = await resp.Content.ReadAsStringAsync();
                return respText;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"發生錯誤: {ex.Message}");
                return "";
            }
        }

        public override async Task<string> SpeechToTextA(
            ReadOnlyMemory<byte> audioData, string mimeType, CancellationToken ct = default)
        {
            if (audioData.IsEmpty)
                throw new ArgumentException("Audio data is empty.", nameof(audioData));
            if (string.IsNullOrWhiteSpace(mimeType))
                throw new ArgumentException("Audio MIME type is required.", nameof(mimeType));
            if (string.IsNullOrWhiteSpace(_llmUrl))
                throw new InvalidOperationException("Gemini GenerateContent URL is not configured.");

            var requestMimeType = mimeType;
            var requestAudio = audioData.ToArray();
            if (mimeType.StartsWith("audio/pcm", StringComparison.OrdinalIgnoreCase))
            {
                requestAudio = AddPcmWavHeader(audioData.Span);
                requestMimeType = "audio/wav";
            }

            var request = new
            {
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new { text = "請忠實轉錄音訊中的語音，只輸出逐字稿，不要回答或補充說明。若沒有可辨識的人聲，只輸出 [SILENCE]。" }
                    }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new object[]
                        {
                            new { inlineData = new { mimeType = requestMimeType, data = Convert.ToBase64String(requestAudio) } }
                        }
                    }
                }
            };

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(SpeechToTextTimeout);

            using var resp = await _httpClient.PostAsJsonAsync(_llmUrl, request, timeoutCts.Token);
            var respText = await resp.Content.ReadAsStringAsync(timeoutCts.Token);
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"Gemini speech transcription failed ({(int)resp.StatusCode}): {respText}");

            var respJson = JObject.Parse(respText);
            var tranScript = string.Concat(
                respJson["candidates"]?[0]?["content"]?["parts"]?
                    .Select(part => part["text"]?.Value<string>() ?? string.Empty)
                ?? Enumerable.Empty<string>()).Trim();

            return tranScript.Contains("[SILENCE]", StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : tranScript;
        }

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

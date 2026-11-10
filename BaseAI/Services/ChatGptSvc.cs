using BaseAI.Interfaces;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HttpMethod = System.Net.Http.HttpMethod;

namespace BaseAI.Services
{
    /// <summary>透過 OpenAI HTTP API 處理文字請求與 WAV 轉錄。</summary>
    /// <param name="llmUrl">文字請求的 HTTP 端點。</param>
    /// <param name="embedUrl">傳給基底服務的文字嵌入端點，可為空字串。</param>
    /// <param name="apiKey">語音轉錄請求使用的 API 金鑰。</param>
    /// <param name="transcriptionUrl">音訊轉錄的 HTTP 端點。</param>
    /// <param name="transcriptionModel">音訊轉錄使用的模型名稱。</param>
    public class ChatGptSvc(
        string llmUrl,
        string embedUrl = "",
        string apiKey = "",
        string transcriptionUrl = "https://api.openai.com/v1/audio/transcriptions",
        string transcriptionModel = "gpt-4o-mini-transcribe") : AbLlmSvc(llmUrl, embedUrl)
    {
        /*
        const string UrlApi = "https://api.openai.com/v1/responses";
        const string ModelType = "gpt-5.4-mini";

        private readonly HttpClient _httpClient;
        //private readonly string _apiKey;

        public ChatGptSvc()
        {
            _httpClient = new HttpClient();
            //_apiKey = apiKey;
        }
        */

        /// <summary>送出文字請求，成功時回傳原始 HTTP 回應本文。</summary>
        /// <param name="prompt">傳給模型的系統提示詞。</param>
        /// <param name="question">傳給模型的使用者輸入。</param>
        public override async Task<string> AskA(string prompt, string question)
        {
            //_httpClient.DefaultRequestHeaders.Clear();
            //_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var body = new
            {
                //model = ModelType,
                input = new object[]
                {
                    new
                    {
                        role = "system",
                        content = new object[]
                        {
                            new { type = "input_text", text = prompt }
                        }
                    },
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "input_text", text = question }
                        }
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _llmUrl);
            //request.Headers.Authorization =
            //    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = JsonContent.Create(body);

            var resp = await _httpClient.SendAsync(request);
            var text = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"HTTP {(int)resp.StatusCode}: {text}");

            return text;

            /*
            var resp = await _httpClient.PostAsJsonAsync(UrlApi, body);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadAsStringAsync();
            */
        }

        /// <summary>驗證完整 WAV 後送出轉錄請求，回傳去除首尾空白的文字。</summary>
        /// <param name="audioData">要轉錄的完整 WAV 位元組。</param>
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
            ArgumentException.ThrowIfNullOrWhiteSpace(mimeType);
            if (mimeType.Split(';', 2)[0].Trim().ToLowerInvariant() is not ("audio/wav" or "audio/x-wav" or "audio/wave"))
                throw new NotSupportedException("OpenAI STT currently accepts WAV files only; convert the audio first.");
            _Voice.Validate(audioData.Span);
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("OpenAI API key is not configured.");

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(transcriptionModel), "model");
            // OpenAI 的 prompt 欄位用於轉錄背景與詞彙提示。
            var context = string.Join("\n", new[] { prompt, string.Join(", ", keywords) }.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (context.Length > 0) form.Add(new StringContent(context), "prompt");
            var fileContent = new ByteArrayContent(audioData.ToArray());
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
            form.Add(fileContent, "file", "audio.wav");

            using var request = new HttpRequestMessage(HttpMethod.Post, transcriptionUrl)
            {
                Content = form
            };
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            using var response = await _httpClient.SendAsync(request, ct);
            var responseText = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    "OpenAI speech transcription failed.", null, response.StatusCode);

            return JObject.Parse(responseText)["text"]?.Value<string>()?.Trim() ?? string.Empty;
        }

        //todo
        /// <summary>目前尚未實作文字嵌入，一律回傳 null。</summary>
        /// <param name="text">預留的文字輸入，現行實作未讀取。</param>
        public override async Task<float[]?> TextToVectorA(string text)
        {
            return null;
        }
    }
}

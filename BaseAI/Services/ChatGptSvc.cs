using BaseAI.Interfaces;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HttpMethod = System.Net.Http.HttpMethod;

namespace BaseAI.Services
{
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

        public override async Task<string> SpeechToTextA(
            ReadOnlyMemory<byte> audioData, string mimeType, CancellationToken ct = default)
        {
            if (audioData.IsEmpty)
                throw new ArgumentException("Audio data is empty.", nameof(audioData));
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("OpenAI API key is not configured.");

            var extension = mimeType.ToLowerInvariant() switch
            {
                "audio/wav" or "audio/wave" => "wav",
                "audio/mpeg" or "audio/mp3" => "mp3",
                "audio/mp4" or "audio/m4a" => "m4a",
                "audio/webm" => "webm",
                "audio/ogg" => "ogg",
                _ => throw new NotSupportedException($"OpenAI transcription does not support '{mimeType}'.")
            };

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(transcriptionModel), "model");
            var fileContent = new ByteArrayContent(audioData.ToArray());
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
            form.Add(fileContent, "file", $"audio.{extension}");

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
                    $"OpenAI speech transcription failed ({(int)response.StatusCode}): {responseText}");

            return JObject.Parse(responseText)["text"]?.Value<string>()?.Trim() ?? string.Empty;
        }

        //todo
        public override async Task<float[]?> TextToVectorA(string text)
        {
            return null;
        }
    }
}

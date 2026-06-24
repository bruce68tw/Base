using AngleSharp.Dom;
using Base.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using HttpMethod = System.Net.Http.HttpMethod;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Wordprocessing;

namespace LlmChatGpt
{
    public class GeminiSvc : ILLMSvc
    {
        const string UrlApi = "https://generativelanguage.googleapis.com/v1beta/models";
        const string ModelType = "gemini-1.5-flash";

        //string model = "gemini-1.5-flash";
        //string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiSvc(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<string> AskA(string prompt, string question)
        {
            //_httpClient.DefaultRequestHeaders.Clear();
            //_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var body = new
            {
                // 這裡設定系統提示詞
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                },
                // 這裡設定使用者的對話內容
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = question }
                        }
                    }
                }
            };

            // 3. 序列化並發送請求
            string bodyText = JsonSerializer.Serialize(body);
            var content = new StringContent(bodyText, Encoding.UTF8, "application/json");

            var url = $"{UrlApi}/{ModelType}:generateContent?key={_apiKey}";

            try
            {
                var resp = await _httpClient.PostAsync(url, content);
                resp.EnsureSuccessStatusCode();
                var respText = await resp.Content.ReadAsStringAsync();
                return respText;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"發生錯誤: {ex.Message}");
                return "";
            }

            /*
            var request = new HttpRequestMessage(HttpMethod.Post, UrlApi);
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = JsonContent.Create(body);

            var resp = await _httpClient.SendAsync(request);
            var respText = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"HTTP {(int)resp.StatusCode}: {respText}");

            return respText;
            */
        }
    }
}

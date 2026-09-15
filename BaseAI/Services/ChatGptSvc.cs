using BaseAI.Interfaces;
using System.Net.Http.Json;
using HttpMethod = System.Net.Http.HttpMethod;

namespace BaseAI.Services
{
    public class ChatGptSvc(string llmUrl, string embedUrl = "") : AbLlmSvc(llmUrl, embedUrl)
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

        //todo
        public override async Task<float[]?> TextToVectorA(string text)
        {
            return null;
        }
    }
}

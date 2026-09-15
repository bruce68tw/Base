using Base.Services;
using BaseAI.Interfaces;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BaseAI.Services
{
    public class GeminiSvc(string llmUrl, string embedUrl = "", int embedDim = 0) : AbLlmSvc(llmUrl, embedUrl, embedDim)
    {

        //const string UrlApi = "https://generativelanguage.googleapis.com/v1beta/models";
        //const string ModelType = "gemini-3-flash";

        /*
        private readonly HttpClient _httpClient;
        private string _llmUrl = "";
        private string _embedUrl = "";

        public GeminiSvc()
        {
            _httpClient = new HttpClient();
        }

        public void Init(string llmUrl, string embedUrl = "")
        {
            _llmUrl = llmUrl;
            _embedUrl = embedUrl;
        }
        */

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

using Base.Services;
using BaseAI.Interfaces;
using BaseAI.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BaseAI.Services
{
    //Chroma 使用 http 連線, 無需第3方套件
    public class ChromaSvc(string embedDbStr, string embedTableName, int embedDim) : 
        AbEmbedDbSvc(embedDbStr, embedTableName, embedDim)
    {
        private string _nowCollectTable = "";

        private async Task<string> GetCollectIdA(string table)
        {
            const string preFun = "ChromaSvc.cs GetCollectId() failed: ";
            using var resp = await _httpClient.GetAsync($"{_embedDbStr}/{table}");
            if (!resp.IsSuccessStatusCode)
            {
                _Log.Error(preFun + await resp.Content.ReadAsStringAsync());
                return "";
            }

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("id").GetString()!;
        }

        public override async Task<bool> CreateA(string tableName, string id, float[] vector, string fileId)
        {
            //connect table, 有 using var 所以不使用 goto
            const string preFun = "ChromaSvc.cs CreateA() failed: ";
            var collectId = await GetCollectIdA(tableName);
            if (string.IsNullOrEmpty(collectId))
                return false;
            /*
            var resp = await _httpClient.GetAsync($"{_embedDbStr}/{_embedTableName}");
            if (!resp.IsSuccessStatusCode)
            {
                _Log.Error(preFun + await resp.Content.ReadAsStringAsync());
                return false;
            }

            //check table existed
            var collect = JObject.Parse(await resp.Content.ReadAsStringAsync());
            var collectId = collect["id"]?.ToString();
            if (string.IsNullOrWhiteSpace(collectId))
            {
                _Log.Error(preFun + "Chroma collection Id missing.");
                return false;
            }
            */

            //prepare row
            /*
            var payload = new JObject
            {
                ["ids"] = new JArray(id),
                ["documents"] = new JArray(title),
                ["embeddings"] = JArray.FromObject(new[] { vector })
            };
            */
            var json = new
            {
                ids = new[] { id },
                embeddings = new[] { vector },
                metadatas = new[]{
                    new Dictionary<string, object>{["FileId"] = fileId}
                }
            };

            //if (metadata != null)
            //    payload["metadatas"] = JArray.FromObject(new[] { metadata });

            //using var content = new StringContent(payload.ToString(Newtonsoft.Json.Formatting.None),
            //    Encoding.UTF8, "application/json");

            //write row
            using var resp2 = await _httpClient.PostAsJsonAsync($"{_embedDbStr}/{collectId}/add", json);
            if (!resp2.IsSuccessStatusCode)
            {
                _Log.Error(preFun + await resp2.Content.ReadAsStringAsync());
                return false;
            }

            //case of
            return true;
        }

        public override Task<EmbedDocuDto?> GetA(string id)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteA(string id)
        {
            throw new NotImplementedException();
        }

        public override async Task<string> VectorToTextA(string tableName, float[] vector  /*, int topK = 5*/)
        {
            var resp = await _httpClient.GetAsync($"{_embedDbStr}/{tableName}");
            if (!resp.IsSuccessStatusCode)
                return $"Error: Chroma collection '{tableName}' not found.";

            var row = JObject.Parse(await resp.Content.ReadAsStringAsync());
            var colId = row["id"]?.ToString();
            if (string.IsNullOrEmpty(colId))
                return "Error: Chroma collection ID missing.";

            // 執行 Query
            var queryUrl = $"{_embedDbStr}/{colId}/query";
            var queryPayload = new
            {
                query_embeddings = new[] { vector },
                n_results = 4
            };

            var queryJson = JObject.FromObject(queryPayload).ToString(Newtonsoft.Json.Formatting.None);
            using var queryContent = new StringContent(queryJson, Encoding.UTF8, "application/json");

            var queryResponse = await _httpClient.PostAsync(queryUrl, queryContent);
            if (!queryResponse.IsSuccessStatusCode)
                return "Error: Chroma query failed.";

            var resJson = JObject.Parse(await queryResponse.Content.ReadAsStringAsync());
            var documents = resJson["documents"]?[0] as JArray;
            if (documents == null || documents.Count == 0)
                return "查無相關資訊。";

            return string.Join("\n", documents.Select(d => d.ToString()));
        }

        public override Task ClearA()
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> DeleteByCondA(string tableName, string fid, string value)
        {
            const string preFun = "ChromaSvc.cs DeleteByCondA() failed: ";
            var collectId = await GetCollectIdA(tableName);
            if (string.IsNullOrEmpty(collectId))
                return false;

            var json = new
            {
                where = new Dictionary<string, string> { [fid] = value }
            };

            //using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var resp = await _httpClient.PostAsJsonAsync(
                $"{_embedDbStr}/{collectId}/delete", json);

            if (!resp.IsSuccessStatusCode)
            {
                //throw new Exception(await resp.Content.ReadAsStringAsync());
                _Log.Error(preFun + "delete 失敗。");
                return false;
            }

            //case ok
            return true;
        }
    }
}

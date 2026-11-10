using Base.Services;
using BaseAI.Interfaces;
using BaseAI.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace BaseAI.Services
{
    /// <summary>透過 Chroma HTTP API 讀寫向量集合。</summary>
    public class ChromaSvc : AbEmbedDbSvc
    {
        /// <summary>建立 Chroma HTTP 向量資料庫服務。</summary>
        /// <param name="embedDbStr">Chroma 集合 API 的根位址。</param>
        /// <param name="embedTable">未指定集合時採用的名稱。</param>
        /// <param name="embedDim">向量預期維度。</param>
        /// <param name="maxDistance">相似度查詢的預設最大距離。</param>
        /// <param name="queryRows">相似度查詢的預設候選筆數。</param>
        public ChromaSvc(string embedDbStr, string embedTable, int embedDim,
            double maxDistance, int queryRows) :
            base(embedDbStr, embedTable, embedDim, maxDistance, queryRows)
        {
        }

        //private string _nowTableId = "";

        protected override async Task<bool> SetNowTableA(string table)
        {
            if (table == _nowTable) return true;

            try
            {
                using var resp = await _httpClient.GetAsync($"{_embedDbStr}/{table}");
                if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    //case create new
                    var payload = new
                    {
                        name = table,
                        metadata = new Dictionary<string, string> { ["hnsw:space"] = "cosine" }
                    };
                    using var createResp = await _httpClient.PostAsJsonAsync(_embedDbStr, payload);
                    if (!createResp.IsSuccessStatusCode)
                    {
                        _Log.Error("ChromaSvc: create collection failed: " +
                            await createResp.Content.ReadAsStringAsync());
                        return false;
                    }

                    return await ResponseToTableA(table,  createResp);
                }
                else if (!resp.IsSuccessStatusCode)
                {
                    _Log.Error("ChromaSvc: get collection failed: " +
                        await resp.Content.ReadAsStringAsync());
                    return false;
                }

                //case ok
                return await ResponseToTableA(table, resp);
            }
            catch (Exception ex)
            {
                _Log.Error("ChromaSvc: resolve collection failed: " + ex.Message);
                return false;
            }
        }

        private async Task<bool> ResponseToTableA(string table, HttpResponseMessage resp)
        {
            using var json = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var tableId = json.RootElement.TryGetProperty("id", out var id)
                ? id.GetString() ?? ""
                : "";
            var status = (tableId != "");
            if (status)
            {
                _nowTable = table;
                _nowTableId = tableId;
            }
            return status;
        }

        /*
        private bool HasExpectedDimension(float[] vector)
        {
            if (_embedDim <= 0 || vector.Length == _embedDim)
                return true;

            _Log.Error($"ChromaSvc: embedding dimension mismatch; expected {_embedDim}, got {vector.Length}.");
            return false;
        }
        */
        /// <summary>將向量與來源文件 ID 新增至指定集合。</summary>
        /// <param name="table">目標集合名稱，空字串時使用預設集合。</param>
        /// <param name="id">要新增的項目 ID。</param>
        /// <param name="vector">要保存的嵌入向量。</param>
        /// <param name="fileId">寫入中繼資料的來源文件 ID。</param>
        public override async Task<bool> CreateA(string table, string id, float[] vector, string fileId)
        {
            //if (!HasExpectedDimension(vector)) return false;
            if (!await SetNowTableA(table)) return false;

            var payload = new
            {
                ids = new[] { id },
                embeddings = new[] { vector },
                metadatas = new[] { new Dictionary<string, object> { ["FileId"] = fileId } }
            };

            using var resp = await _httpClient.PostAsJsonAsync(ActionUrl("add"), payload);
            if (!resp.IsSuccessStatusCode)
            {
                _Log.Error("ChromaSvc: create vector failed: " +
                    await resp.Content.ReadAsStringAsync());
                return false;
            }

            return true;
        }

        /// <summary>更新指定集合中的向量與來源文件 ID。</summary>
        /// <param name="table">目標集合名稱，空字串時使用預設集合。</param>
        /// <param name="id">要更新的項目 ID。</param>
        /// <param name="vector">用來更新項目的嵌入向量。</param>
        /// <param name="fileId">寫入中繼資料的來源文件 ID。</param>
        public override async Task<bool> UpdateA(string table, string id, float[] vector, string fileId)
        {
            //if (!HasExpectedDimension(vector)) return false;
            if (!await SetNowTableA(table)) return false;

            var payload = new
            {
                ids = new[] { id },
                embeddings = new[] { vector },
                metadatas = new[] { new Dictionary<string, object> { ["FileId"] = fileId } }
            };

            using var resp = await _httpClient.PostAsJsonAsync(ActionUrl("update"), payload);
            if (!resp.IsSuccessStatusCode)
            {
                _Log.Error("ChromaSvc: update vector failed: " +
                    await resp.Content.ReadAsStringAsync());
                return false;
            }

            return true;
        }

        /// <summary>依 ID 讀回向量項目，查無資料時回傳 null。</summary>
        /// <param name="table">來源集合名稱，空字串時使用預設集合。</param>
        /// <param name="id">要查詢的項目 ID。</param>
        public override async Task<EmbedRowDto?> GetA(string table, string id)
        {
            if (!await SetNowTableA(table)) return null;

            var payload = new
            {
                ids = new[] { id },
                include = new[] { "documents", "embeddings", "metadatas" }
            };
            using var resp = await _httpClient.PostAsJsonAsync(ActionUrl("get"), payload);
            if (!resp.IsSuccessStatusCode)
            {
                _Log.Error("ChromaSvc: get vector failed: " +
                    await resp.Content.ReadAsStringAsync());
                return null;
            }

            using var document = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var root = document.RootElement;
            if (!root.TryGetProperty("ids", out var ids) || ids.GetArrayLength() == 0)
                return null;

            var result = new EmbedRowDto { Id = ids[0].GetString() ?? id };
            if (root.TryGetProperty("documents", out var documents) &&
                documents.GetArrayLength() > 0 && documents[0].ValueKind != JsonValueKind.Null)
                result.Text = documents[0].GetString() ?? "";

            if (root.TryGetProperty("embeddings", out var embeddings) &&
                embeddings.GetArrayLength() > 0 && embeddings[0].ValueKind == JsonValueKind.Array)
                result.Embedding = embeddings[0].EnumerateArray()
                    .Select(value => value.GetSingle()).ToArray();

            if (root.TryGetProperty("metadatas", out var metadatas) &&
                metadatas.GetArrayLength() > 0 && metadatas[0].ValueKind == JsonValueKind.Object)
                result.Metadata = metadatas[0].EnumerateObject()
                    .ToDictionary(property => property.Name, property => (object)property.Value.Clone());

            return result;
        }

        /// <summary>從指定集合刪除單一向量項目。</summary>
        /// <param name="table">目標集合名稱，空字串時使用預設集合。</param>
        /// <param name="id">要刪除的項目 ID。</param>
        public override async Task<bool> DeleteA(string table, string id)
        {
            if (!await SetNowTableA(table)) return false;

            using var response = await _httpClient.PostAsJsonAsync(ActionUrl("delete"), new { ids = new[] { id } });
            if (!response.IsSuccessStatusCode)
            {
                _Log.Error("ChromaSvc: delete vector failed: " +
                    await response.Content.ReadAsStringAsync());
                return false;
            }

            return true;
        }

        //private sealed record QueryMatch(string Id, double Distance, string Content);

        /*
        private async Task<List<QueryMatch>> QueryA(
            float[] vector, decimal maxDistance, int queryRows, string table)
        {
            if (!HasExpectedDimension(vector)) return [];
            if (!await SetNowTableA(table, create: false)) return [];

            var rowCount = queryRows > 0 ? queryRows : _queryRows;
            var distanceLimit = maxDistance > 0 ? maxDistance : _maxDistance;
            var payload = new
            {
                query_embeddings = new[] { vector },
                n_results = Math.Max(1, rowCount),
                include = new[] { "distances", "documents" }
            };

            using var response = await _httpClient.PostAsJsonAsync(ActionUrl("query"), payload);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _Log.Error("ChromaSvc: similarity query failed: " + error);
                return [];
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = document.RootElement;
            if (!root.TryGetProperty("ids", out var idGroups) || idGroups.GetArrayLength() == 0 ||
                !root.TryGetProperty("distances", out var distanceGroups) ||
                distanceGroups.GetArrayLength() == 0 ||
                !root.TryGetProperty("documents", out var documentGroups) ||
                documentGroups.GetArrayLength() == 0)
                return [];

            var ids = idGroups[0];
            var distances = distanceGroups[0];
            var contents = documentGroups[0];
            var matchCount = Math.Min(ids.GetArrayLength(),
                Math.Min(distances.GetArrayLength(), contents.GetArrayLength()));
            var matches = new List<QueryMatch>(matchCount);
            for (var index = 0; index < matchCount; index++)
            {
                var distance = distances[index].GetDouble();
                if (distanceLimit > 0 && (decimal)distance > distanceLimit)
                    continue;

                matches.Add(new QueryMatch(
                    ids[index].GetString() ?? "",
                    distance,
                    contents[index].ValueKind == JsonValueKind.Null
                        ? ""
                        : contents[index].GetString() ?? ""));
            }

            return matches;
        }
        */

        /// <summary>查詢相似向量並回傳通過距離門檻的項目 ID。</summary>
        /// <param name="table">查詢的集合名稱，空字串時使用預設集合。</param>
        /// <param name="vector">查詢用的嵌入向量。</param>
        /// <param name="maxDistance">允許的最大距離，為零時採用服務預設值。</param>
        /// <param name="queryRows">向 Chroma 要求的候選筆數，非正數時採用服務預設值。</param>
        public override async Task<List<string>> VectorToIdsA(string table, float[] vector, 
            double maxDistance = 0, int queryRows = 0)
        {
            //var targetTable = string.IsNullOrWhiteSpace(table) ? _embedTable : table;
            if (!await SetNowTableA(table)) return [];

            var payload = new
            {
                query_embeddings = new[] { vector },
                n_results = queryRows > 0 ? queryRows : _queryRows,
                include = new[] { "distances" }
            };

            using var resp = await _httpClient.PostAsJsonAsync(ActionUrl("query"), payload);
            if (!resp.IsSuccessStatusCode)
            {
                _Log.Error("ChromaSvc: similarity query failed: " +
                    await resp.Content.ReadAsStringAsync());
                return [];
            }

            using var document = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var root = document.RootElement;
            if (!root.TryGetProperty("ids", out var idGroups) || idGroups.GetArrayLength() == 0 ||
                !root.TryGetProperty("distances", out var distanceGroups) || distanceGroups.GetArrayLength() == 0)
                return [];

            if (maxDistance == 0) maxDistance = _maxDistance;
            var ids = idGroups[0];
            var distances = distanceGroups[0];
            //var distanceLimit = maxDistance > 0 ? maxDistance : _maxDistance;
            var results = new List<string>();
            for (var i = 0; i < Math.Min(ids.GetArrayLength(), distances.GetArrayLength()); i++)
            {
                if (distances[i].GetDouble() <= maxDistance)
                {
                    var id = ids[i].GetString();
                    if (!string.IsNullOrEmpty(id)) results.Add(id);
                }
            }

            return results;
        }

        /*
        public override async Task<string> VectorToTextA(
            float[] vector, decimal maxDistance = 0, int queryRows = 0)
        {
            var matches = await QueryA(vector, maxDistance, queryRows, table);
            return string.Join(Environment.NewLine + Environment.NewLine,
                matches.Select(match => match.Content).Where(content => !string.IsNullOrWhiteSpace(content)));
        }
        */

        /// <summary>依中繼資料中的 FileId 刪除屬於同一來源文件的向量。</summary>
        /// <param name="table">目標集合名稱，空字串時使用預設集合。</param>
        /// <param name="fileId">要比對的來源文件 ID。</param>
        public override async Task<bool> DeleteByFileIdA(string table, string fileId)
        {
            if (!await SetNowTableA(table)) return false;

            var payload = new { where = new Dictionary<string, string> { ["FileId"] = fileId } };
            using var response = await _httpClient.PostAsJsonAsync(ActionUrl("delete"), payload);
            if (!response.IsSuccessStatusCode)
            {
                _Log.Error("ChromaSvc: delete vectors by file ID failed: " +
                    await response.Content.ReadAsStringAsync());
                return false;
            }

            return true;
        }

        /*
        public override async Task ClearA()
        {
            if (!await SetNowTableA(_embedTable, create: false)) return;

            using var getResponse = await _httpClient.PostAsJsonAsync(
                ActionUrl(_nowTableId, "get"), new { include = Array.Empty<string>() });
            if (!getResponse.IsSuccessStatusCode)
                throw new HttpRequestException("Chroma get IDs failed: " +
                    await getResponse.Content.ReadAsStringAsync());

            using var document = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
            if (!document.RootElement.TryGetProperty("ids", out var ids) || ids.GetArrayLength() == 0)
                return;

            var idList = ids.EnumerateArray().Select(value => value.GetString()).ToArray();
            using var deleteResponse = await _httpClient.PostAsJsonAsync(
                ActionUrl(_nowTableId, "delete"), new { ids = idList });
            if (!deleteResponse.IsSuccessStatusCode)
                throw new HttpRequestException("Chroma clear collection failed: " +
                    await deleteResponse.Content.ReadAsStringAsync());
        }
        */

    }
}
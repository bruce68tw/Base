using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BaseAI.Services
{
    // ==========================================
    // 1. 資料模型定義 (Models)
    // ==========================================

    public class Document
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Content { get; set; } = string.Empty;
        public double Score { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class ContextPacket
    {
        public List<Document> Items { get; set; } = new();
        public int TotalTokens { get; set; }
        public string SystemPrompt { get; set; } = string.Empty;

        public string Compile()
        {
            var contentList = Items.Select(x => x.Content);
            return string.Join("\n---\n", contentList);
        }
    }

    // ==========================================
    // 2. 核心元件介面 (Interfaces)
    // ==========================================

    public interface IRetriever
    {
        Task<List<Document>> RetrieveAsync(string query, int topK);
    }

    public interface IReRanker
    {
        Task<List<Document>> ReRankAsync(string query, List<Document> documents);
    }

    public interface IMemory
    {
        Task<List<Document>> GetShortTermMemoryAsync();
        Task AddMemoryAsync(string content);
    }

    public interface ICompressor
    {
        Task<string> CompressAsync(string content, int maxTokens);
    }

    public interface ITokenBudget
    {
        int EstimateTokens(string text);
        ContextPacket FitToBudget(List<Document> documents, int maxBudget);
    }

    // ==========================================
    // 3. 預設實作 (Default Implementations)
    // ==========================================

    /// <summary>
    /// 1. Retriever: 從資料源檢索相關文件
    /// </summary>
    public class MockRetriever : IRetriever
    {
        private readonly List<Document> _database;

        public MockRetriever(List<Document> database)
        {
            _database = database;
        }

        public async Task<List<Document>> RetrieveAsync(string query, int topK)
        {
            await Task.Delay(10); // 模擬非同步 I/O
            // 簡單以包含關鍵字作為模擬檢索
            return _database
                .Where(d => d.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(topK)
                .ToList();
        }
    }

    /// <summary>
    /// 2. Re-ranker: 對檢索結果進行重新排序與評分
    /// </summary>
    public class SimpleReRanker : IReRanker
    {
        public async Task<List<Document>> ReRankAsync(string query, List<Document> documents)
        {
            await Task.Delay(10);
            // 模擬 Cross-encoder 重新計分邏輯
            foreach (var doc in documents)
            {
                doc.Score = doc.Content.Contains(query) ? 0.95 : 0.50;
            }
            return documents.OrderByDescending(d => d.Score).ToList();
        }
    }

    /// <summary>
    /// 3. Memory: 注入短期對話記憶或使用者偏好
    /// </summary>
    public class InMemoryStore : IMemory
    {
        private readonly List<Document> _memories = new();

        public async Task<List<Document>> GetShortTermMemoryAsync()
        {
            await Task.CompletedTask;
            return _memories;
        }

        public async Task AddMemoryAsync(string content)
        {
            await Task.CompletedTask;
            _memories.Add(new Document { Content = content, Score = 1.0 });
        }
    }

    /// <summary>
    /// 4. Compressor: 壓縮過長的文件內容以節省空間
    /// </summary>
    public class BasicCompressor : ICompressor
    {
        public async Task<string> CompressAsync(string content, int maxTokens)
        {
            await Task.CompletedTask;
            // 簡易字串截斷模擬壓縮（實際可接 LLM 摘要）
            int maxChars = maxTokens * 4; // 粗略估算 1 token ≒ 4 字元
            if (content.Length > maxChars)
            {
                return content.Substring(0, maxChars) + "...[Compressed]";
            }
            return content;
        }
    }

    /// <summary>
    /// 5. TokenBudget: 控管與裁剪 Context 以符合 Token 上限
    /// </summary>
    public class TokenBudgetManager : ITokenBudget
    {
        public int EstimateTokens(string text)
        {
            // 簡易估算：英文以空白切分，中文以字數粗估 (1 token ≒ 3-4 bytes)
            return string.IsNullOrEmpty(text) ? 0 : (int)Math.Ceiling(text.Length / 3.0);
        }

        public ContextPacket FitToBudget(List<Document> documents, int maxBudget)
        {
            var packet = new ContextPacket();
            int currentTokens = 0;

            foreach (var doc in documents)
            {
                int docTokens = EstimateTokens(doc.Content);
                if (currentTokens + docTokens <= maxBudget)
                {
                    packet.Items.Add(doc);
                    currentTokens += docTokens;
                }
                else
                {
                    // 若部分超出，可選擇捨棄或進一步處理
                    break;
                }
            }

            packet.TotalTokens = currentTokens;
            return packet;
        }
    }

    // ==========================================
    // 6. Context Engine 總協調器 (Orchestrator)
    // ==========================================

    public class ContextEnginePipeline
    {
        private readonly IRetriever _retriever;
        private readonly IReRanker _reRanker;
        private readonly IMemory _memory;
        private readonly ICompressor _compressor;
        private readonly ITokenBudget _tokenBudget;

        public ContextEnginePipeline(
            IRetriever retriever,
            IReRanker reRanker,
            IMemory memory,
            ICompressor compressor,
            ITokenBudget tokenBudget)
        {
            _retriever = retriever;
            _reRanker = reRanker;
            _memory = memory;
            _compressor = compressor;
            _tokenBudget = tokenBudget;
        }

        public async Task<ContextPacket> BuildContextAsync(string query, int maxTokenBudget)
        {
            // Step 1: Retriever
            var retrievedDocs = await _retriever.RetrieveAsync(query, topK: 5);

            // Step 2: Re-ranker
            var rankedDocs = await _reRanker.ReRankAsync(query, retrievedDocs);

            // Step 3: Memory (加入短期記憶)
            var memories = await _memory.GetShortTermMemoryAsync();
            var combinedDocs = memories.Concat(rankedDocs).ToList();

            // Step 4: Compressor (對過長文件進行壓縮)
            foreach (var doc in combinedDocs)
            {
                doc.Content = await _compressor.CompressAsync(doc.Content, maxTokens: 500);
            }

            // Step 5: TokenBudget (組裝並確保符合 Token 限制)
            var packet = _tokenBudget.FitToBudget(combinedDocs, maxTokenBudget);

            // Step 6: ContextPacket 準備完成
            return packet;
        }
    }
}
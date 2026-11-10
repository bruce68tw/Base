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

    /// <summary>供上下文處理流程檢索、評分及選入的文件。</summary>
    public class Document
    {
        /// <summary>文件識別值，預設產生新的 GUID。</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>用於檢索、壓縮與組裝上下文的文件文字。</summary>
        public string Content { get; set; } = string.Empty;
        /// <summary>重排器設定的相關性分數。</summary>
        public double Score { get; set; }
        /// <summary>呼叫端可隨文件傳遞的附加欄位。</summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>包含入選文件與估算用量的上下文結果。</summary>
    public class ContextPacket
    {
        /// <summary>依處理順序選入的文件清單。</summary>
        public List<Document> Items { get; set; } = new();
        /// <summary>入選文件內容的估算權杖總數。</summary>
        public int TotalTokens { get; set; }
        /// <summary>呼叫端可附帶的系統提示詞，Compile 目前只讀取 Items。</summary>
        public string SystemPrompt { get; set; } = string.Empty;

        /// <summary>依清單順序將文件內容串成文字，文件間以分隔線隔開。</summary>
        public string Compile()
        {
            var contentList = Items.Select(x => x.Content);
            return string.Join("\n---\n", contentList);
        }
    }

    // ==========================================
    // 2. 核心元件介面 (Interfaces)
    // ==========================================

    /// <summary>依查詢文字找出候選文件的契約。</summary>
    public interface IRetriever
    {
        /// <summary>檢索與輸入相關的候選文件。</summary>
        /// <param name="query">要檢索的文字。</param>
        /// <param name="topK">最多取回的候選筆數。</param>
        Task<List<Document>> RetrieveAsync(string query, int topK);
    }

    /// <summary>依查詢重新排列候選文件的契約。</summary>
    public interface IReRanker
    {
        /// <summary>依查詢重新評分並排序候選文件。</summary>
        /// <param name="query">作為評分依據的查詢。</param>
        /// <param name="documents">要重新排序的候選文件。</param>
        Task<List<Document>> ReRankAsync(string query, List<Document> documents);
    }

    /// <summary>保存與讀取短期記憶的契約。</summary>
    public interface IMemory
    {
        /// <summary>取得供組裝上下文使用的短期記憶。</summary>
        Task<List<Document>> GetShortTermMemoryAsync();
        /// <summary>將文字加入短期記憶。</summary>
        /// <param name="content">要保存的文字。</param>
        Task AddMemoryAsync(string content);
    }

    /// <summary>依權杖預算縮減文件內容的契約。</summary>
    public interface ICompressor
    {
        /// <summary>縮減文字以符合預算。</summary>
        /// <param name="content">要處理的文字。</param>
        /// <param name="maxTokens">縮減目標的權杖上限。</param>
        Task<string> CompressAsync(string content, int maxTokens);
    }

    /// <summary>估算用量並依預算選取文件的契約。</summary>
    public interface ITokenBudget
    {
        /// <summary>估算文字所需的權杖數。</summary>
        /// <param name="text">要估算的文字。</param>
        int EstimateTokens(string text);
        /// <summary>組裝符合預算的上下文結果。</summary>
        /// <param name="documents">候選文件清單。</param>
        /// <param name="maxBudget">可用的權杖上限。</param>
        ContextPacket FitToBudget(List<Document> documents, int maxBudget);
    }

    // ==========================================
    // 3. 預設實作 (Default Implementations)
    // ==========================================

    /// <summary>用記憶體資料與關鍵字子字串比對模擬文件檢索。</summary>
    public class MockRetriever : IRetriever
    {
        private readonly List<Document> _database;

        /// <summary>初始化模擬檢索器使用的文件來源。</summary>
        /// <param name="database">供後續比對的文件清單。</param>
        public MockRetriever(List<Document> database)
        {
            _database = database;
        }

        /// <summary>依原始清單順序取得含有查詢文字的文件，比對時忽略大小寫。</summary>
        /// <param name="query">要比對的子字串。</param>
        /// <param name="topK">最多回傳的符合筆數。</param>
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

    /// <summary>以字串包含判斷模擬文件評分及重排。</summary>
    public class SimpleReRanker : IReRanker
    {
        /// <summary>更新各文件的分數，再依分數由高到低回傳。</summary>
        /// <param name="query">用於區分相關性分數的文字，比對時區分大小寫。</param>
        /// <param name="documents">要評分的候選文件，會原地更新其分數。</param>
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

    /// <summary>使用執行個體內的清單保存短期文字記憶。</summary>
    public class InMemoryStore : IMemory
    {
        private readonly List<Document> _memories = new();

        /// <summary>回傳目前持有的短期記憶清單。</summary>
        public async Task<List<Document>> GetShortTermMemoryAsync()
        {
            await Task.CompletedTask;
            return _memories;
        }

        /// <summary>將文字包裝成文件並加入記憶清單。</summary>
        /// <param name="content">要保存的文字。</param>
        public async Task AddMemoryAsync(string content)
        {
            await Task.CompletedTask;
            _memories.Add(new Document { Content = content, Score = 1.0 });
        }
    }

    /// <summary>依估算的字元上限截斷文件文字。</summary>
    public class BasicCompressor : ICompressor
    {
        /// <summary>依每個權杖四個字元估算上限，超過時截斷並加上標記。</summary>
        /// <param name="content">要檢查或截斷的文字。</param>
        /// <param name="maxTokens">用於估算字元上限的權杖數。</param>
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

    /// <summary>估算文字權杖數，並依文件順序套用總預算。</summary>
    public class TokenBudgetManager : ITokenBudget
    {
        /// <summary>以字元數除以三並向上取整估算用量。</summary>
        /// <param name="text">要估算的文字。</param>
        public int EstimateTokens(string text)
        {
            // 簡易估算：英文以空白切分，中文以字數粗估 (1 token ≒ 3-4 bytes)
            return string.IsNullOrEmpty(text) ? 0 : (int)Math.Ceiling(text.Length / 3.0);
        }

        /// <summary>依輸入順序加入文件，遇到第一筆超出預算的文件即停止。</summary>
        /// <param name="documents">依優先順序排列的候選文件。</param>
        /// <param name="maxBudget">文件內容估算權杖的總上限。</param>
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

    /// <summary>依序協調檢索、重排、記憶、壓縮與預算控制。</summary>
    public class ContextEnginePipeline
    {
        private readonly IRetriever _retriever;
        private readonly IReRanker _reRanker;
        private readonly IMemory _memory;
        private readonly ICompressor _compressor;
        private readonly ITokenBudget _tokenBudget;

        /// <summary>以指定的實作建立上下文處理流程。</summary>
        /// <param name="retriever">查找候選文件的實作。</param>
        /// <param name="reRanker">重新評分與排序的實作。</param>
        /// <param name="memory">提供短期記憶的實作。</param>
        /// <param name="compressor">縮減文件內容的實作。</param>
        /// <param name="tokenBudget">估算用量與選入文件的實作。</param>
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

        /// <summary>執行上下文處理流程，壓縮時會更新所選文件物件的內容。</summary>
        /// <param name="query">用於檢索及重排的輸入文字。</param>
        /// <param name="maxTokenBudget">最終上下文文件的權杖預算。</param>
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
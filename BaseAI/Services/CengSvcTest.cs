namespace BaseAI.Services
{
    public class CengSvcTest
    {
        public async Task RunA()
        {
            // 1. 準備模擬資料庫

            var mockDb = new List<Document>
            {
                new Document { Content = "C# 12 引入了許多新功能，例如頂層陳述式與主建構子。" },
                new Document { Content = "Context Engine 是一個用於優化 LLM 輸入上下文的管線架構。" },
                new Document { Content = ".NET 8 提供了極佳的效能與雲原生支援。" }
            };

            // 2. 初始化各模組
            var retriever = new MockRetriever(mockDb);
            var reRanker = new SimpleReRanker();
            var memory = new InMemoryStore();
            var compressor = new BasicCompressor();
            var tokenBudget = new TokenBudgetManager();

            // 3. 建立管線
            var pipeline = new ContextEnginePipeline(retriever, reRanker, memory, compressor, tokenBudget);

            // 4. 執行建構 Context
            string query = "Context Engine";
            int maxTokens = 1000;

            ContextPacket packet = await pipeline.BuildContextAsync(query, maxTokens);

            // 5. 輸出最終送往 LLM 的 Context
            Console.WriteLine("=== Compiled Context for LLM ===");
            Console.WriteLine(packet.Compile());
            Console.WriteLine($"\nTotal Tokens Used: {packet.TotalTokens}");
        }
    }
}
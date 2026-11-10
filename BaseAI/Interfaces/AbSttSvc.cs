using BaseAI.Models;

namespace BaseAI.Interfaces
{
    /// <summary>
    /// 獨立的語音轉文字契約，不依賴聊天或 Live 工作階段。
    /// 沿用 Interfaces 目錄的 Ab 命名慣例，實際型態為抽象類別。
    /// </summary>
    public abstract class AbSttSvc : IAsyncDisposable
    {
        /// <summary>
        /// 將完整音檔忠實轉錄成文字；明確辨識為無人聲時回傳空字串。
        /// 傳輸、取消、格式或供應商錯誤應拋出例外，不以空字串掩蓋失敗。
        /// </summary>
        /// <param name="audioData">完整且帶容器標頭的音檔，不是任意切割片段或原始 PCM。</param>
        /// <param name="mimeType">與音檔內容相符的 MIME；支援範圍由 provider 決定。</param>
        /// <param name="ct">取消本次轉錄，包括傳送與讀取回應。</param>
        public abstract Task<string> SttA(ReadOnlyMemory<byte> audioData,
            string mimeType, CancellationToken ct = default);

        /// <summary>非同步釋放語音轉文字服務持有的資源。</summary>
        public abstract ValueTask DisposeAsync();
    }
}

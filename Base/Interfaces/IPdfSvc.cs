using Base.Models;

namespace Base.Interfaces
{
    /// <summary>
    /// 提供 Word 轉 PDF 與 PDF 圖片疊加功能；授權與處理細節由實作決定。
    /// </summary>
    public interface IPdfSvc
    {
        /// <summary>
        /// 從 keyPath 指定的檔案設定授權；現有 Spire 實作遇空路徑時略過。
        /// </summary>
        void SetKey(string keyPath);
        /// <summary>
        /// 將 Word 位元組轉為 PDF 位元組；可另提供授權檔路徑，轉換錯誤可能拋出例外。
        /// </summary>
        byte[] WordToPdf(byte[] wordBytes, string keyPath = "");
        /// <summary>
        /// 讀取 fromPath 並將圖片加入後寫至 toPath；現有 Spire 實作只處理第一頁，失敗可能拋出例外。
        /// </summary>
        bool AddImages(string fromPath, string toPath, PdfImageDto[] imageDtos);
    }
}

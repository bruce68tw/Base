using System.IO;

namespace Base.Models
{
    /// <summary>
    /// 自訂檔案下載的結果，包含錯誤或檔案串流。
    /// </summary>
    public class DownFileDto
    {
        /// <summary>
        /// 下載錯誤訊息；使用端先判斷此值。
        /// </summary>
        public string Error { get; set; } = "";
        /// <summary>
        /// 供下載的檔案串流，預設 null。
        /// </summary>
        public Stream? Stream { get; set; } = null;
        /// <summary>
        /// HTTP 回應的 MIME 類型。
        /// </summary>
        public string ContentType { get; set; } = "";
        /// <summary>
        /// 下載顯示的檔名。
        /// </summary>
        public string FileName { get; set; } = "";
    }
}

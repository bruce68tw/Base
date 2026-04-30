using System.IO;

namespace Base.Models
{
    /// <summary>
    /// 用於自訂功能下載檔案的資料傳輸物件，與較單純的 XiFile 不同!!
    /// </summary>
    public class DownFileDto
    {
        public string Error { get; set; } = "";         //錯誤訊息, 會先判斷
        public Stream? Stream { get; set; } = null;     //file stream
        public string ContentType { get; set; } = "";   //MIME
        public string FileName { get; set; } = "";      //檔名
    }
}

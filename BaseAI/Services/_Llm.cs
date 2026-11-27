using Base.Services;
using BaseAI.Models;
using System.Text.Json;

namespace BaseAI.Services
{
    /// <summary>
    /// 檔案讀寫、壓縮、路徑與副檔名處理；未捕捉的檔案系統錯誤仍會拋出。
    /// </summary>
    public class _Llm
    {
        /// <summary>
        /// 讀取 JSON 提問檔；讀取或反序列化失敗時記錄並回傳 null。
        /// _File.ReadLlmAsk -> _Llm.FileToAsk
        /// </summary>
        /// <param name="path">要處理的檔案或目錄路徑</param>
        public static LlmAskDto? FileToAsk(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<LlmAskDto>(json);
            }
            catch (Exception ex)
            {
                _Log.Error("_File.cs ReadLlmAsk() failed: " + ex.Message);
                return null;
            }
        }

    }//class
}

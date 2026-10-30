namespace Base.Models
{
    /// <summary>
    /// 可由 JSON 載入的 LLM 提問內容。
    /// </summary>
    public class LlmAskDto
    {
        /// <summary>
        /// 系統角色的提示文字。
        /// </summary>
        public string System { get; set; } = "";
        /// <summary>
        /// 使用者角色的提問文字。
        /// </summary>
        public string User { get; set; } = "";
    }
}

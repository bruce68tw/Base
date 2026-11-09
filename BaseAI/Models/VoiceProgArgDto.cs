namespace BaseAI.Models
{
    /// <summary>一筆參數，包含語音原文與對應的查詢條件。</summary>
    /// <remarks>
    /// 第一步：AI 填入 VoiceName、VoiceValues，能轉換的值（如日期）先填 Values。
    /// 第二步：畫面載入後，前端提供欄位與下拉選項對照表，AI 填入 Name 並補齊 Values，已填的值保留。
    /// 下拉選項須註明所屬欄位、顯示名稱與代碼；Values 使用該欄位的選項代碼。
    /// 第三步：後端依畫面設定填入 Fid、Operator，不由語音決定運算子。
    /// </remarks>
    public class VoiceProgArgDto
    {
        /// <summary>語音提到的欄位名稱，如「日期」或「客戶」。</summary>
        public string VoiceName { get; set; } = "";

        /// <summary>語音中的原始值，如「這個月」；沒有值時為空集合。</summary>
        public List<string> VoiceValues { get; set; } = [];

        /// <summary>轉換後的查詢值，第一步先填、第二步補齊；尚未確定時為空集合。</summary>
        public List<string> Values { get; set; } = [];

        /// <summary>畫面上的欄位名稱，由第二步 AI 查對照表填入。</summary>
        public string Name { get; set; } = "";

        /// <summary>Name 對應的欄位代碼，由第三步後端填入。</summary>
        //public string Fid { get; set; } = "";

        /// <summary>畫面預設的運算子，由第三步後端填入；使用 <see cref="Base.Enums.QitemOpEstr"/> 的字串值，大小寫須一致。</summary>
        //public string Operator { get; set; } = "";

    }
}

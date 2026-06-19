using Base.Enums;

namespace BaseWeb.Models
{
    /// <summary>
    /// XiText 單行文字輸入欄位的傳入參數類型
    /// </summary>
    public class XiTextDto : XiBaseDto
    {
        /// <summary>
        /// 欄位的最大長度，0表示不限制
        /// </summary>
        public int MaxLen { get; set; } = 0;

        /// <summary>
        /// 是否為password欄位
        /// </summary>
        public bool IsPwd { get; set; } = false;

        /// <summary>
        /// onblur事件的處理函數，會寫任 data-onblur 屬性
        /// </summary>
        public string FnOnBlur { get; set; } = "";

        /// <summary>
        /// 如果有值則輸入右邊會出現find icon，點擊時會開啟 modal 視窗讓用戶尋找相關資料
        /// </summary>
        public string FnOnFind { get; set; } = "";

        /// <summary>
        /// 限制輸入字元，對應後端 InputPatternEstr.cs, 預設可輸入英數字
        /// </summary>
        public string Pattern { get; set; } = InputPatternEstr.EngNumExt;

    }
}
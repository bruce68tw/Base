namespace BaseWeb.Models
{
    /// <summary>
    /// XiCheck 輸入欄位的傳入參數類型
    /// </summary>
    public class XiCheckDto : XiBaseDto
    {
        /// <summary>
        /// 新增一筆資料時此欄位預設是否勾選
        /// </summary>
        public bool IsCheck { get; set; } = false;

        /// <summary>
        /// Checkbox後面的說明文字
        /// </summary>
        public string Label { get; set; } = "";
    }
}
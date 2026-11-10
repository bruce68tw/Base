namespace BaseWeb.Models
{
    //查詢畫面2個日期起迄輸入欄位
    /// <summary>描述查詢畫面的一組起訖日期輸入欄位。</summary>
    public class Date2Dto
    {
        /// <summary>日期範圍欄位顯示的標題。</summary>
        public string Title { get; set; } = "";
        /// <summary>日期範圍使用的基礎欄位識別值。</summary>
        public string FidDate { get; set; } = "";
        /// <summary>日期欄位的版面欄寬設定。</summary>
        public string Cols { get; set; } = "2,8";
    }
}
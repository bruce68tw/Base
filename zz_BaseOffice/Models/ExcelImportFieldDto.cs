
namespace BaseOffice.Models
{
    /// <summary>描述一個 Excel 儲存格對應的匯入欄位。</summary>
    public class ExcelImportFieldDto
    {
        /// <summary>寫入匯入模型的欄位名稱。</summary>
        public string Fid { get; set; } = "";
        //public int Fno { get; set; }
        /// <summary>來源 Excel 儲存格的欄位位置。</summary>
        public string CellName { get; set; } = "";
        /// <summary>是否把儲存格的數值當成 Excel 日期序號。</summary>
        public bool IsDate { get; set; } = false;
    }
}
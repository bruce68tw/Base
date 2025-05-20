
namespace Base.Models
{
    public class ExcelImportFieldDto
    {
        public string Fid { get; set; } = "";
        //public int Fno { get; set; }
        public string CellName { get; set; } = "";
        public bool IsDate { get; set; } = false;
    }
}
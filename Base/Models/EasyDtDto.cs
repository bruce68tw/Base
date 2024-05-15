namespace Base.Models
{
    /// <summary>
    /// easy datatable input argument, use lower camel
    /// refer DtDto.cs
    /// </summary>
    public class EasyDtDto
    {
        /*
        public EasyDtDto()
        {
            start = 0;
            length = 10;
            recordsFiltered = -1;
            findJson = "";
            sort = "";
        }
        */

        //Draw counter, jQuery Datatables need for control async ajax sequence
        //public int draw { get; set; }

        //start row position, base 0
        public int start { get; set; } = 0;

        //rows count to display
        public int length { get; set; } = 10;

        //-1: re-count filtered rows count(means change jquery condition)
        public int recordsFiltered { get; set; } = -1;

        //query condition json in string type
        public string findJson { get; set; } = "";

        //A/D + fid, ex:Au.Account
        public string sort { get; set; } = "";

        //與DtDto.search的資料結構不同, 所以使用不同欄位名稱
        public string search2 { get; set; } = "";

        //search word
        //public DtSearchDto search { get; set; }

    }

}

namespace Base.Models
{
    /// <summary>
    /// for RWD pagin input, use lower camel(jquery simple pagination)
    /// </summary>
    public class PageIn
    {
        public PageIn()
        {
            //start = 0;
            page = 1;
            length = 10;
            filterRows = -1;
            findJson = "";
        }

        //Draw counter, jQuery Datatables need for control async ajax sequence
        //public int draw { get; set; }

        //start row position, base 0
        //public int start { get; set; }
        //page no
        public int page { get; set; }

        //rows count to display
        public int length { get; set; }

        //-1: re-count filtered rows count(means change jquery condition)
        public int filterRows { get; set; }

        //query condition json in string type
        public string findJson { get; set; }

        //search word
        //public DtSearchDto search { get; set; }

        /// <summary>
        /// sorting, get from jQuery DataTables, then get OrderBy, OrderDir
        /// </summary>
        //public List<DtOrderDto> order { get; set; }

    }

}

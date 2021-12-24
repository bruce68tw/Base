using Base.Enums;
using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// for jQuery datatables input argument, use low camel
    /// </summary>
    public class DtDto
    {
        //Draw counter, jQuery Datatables need for control async ajax sequence
        public int draw { get; set; }

        //start row position, base 0
        public int start { get; set; }

        //rows count to display
        public int length { get; set; }

        //-1: re-count filtered rows count(means change jquery condition)
        public int recordsFiltered { get; set; }

        //query condition json in string type
        public string findJson { get; set; }

        //search word
        public DtSearchDto search { get; set; }

        /// <summary>
        /// sorting, get from jQuery DataTables, then get OrderBy, OrderDir
        /// </summary>
        public List<DtOrderDto> order { get; set; }

    }


    //for jQuery datatables 
    public class DtOrderDto
    {
        //sorting column index, base 0
        public int column { get; set; }

        //sorting type: ASC(default), DESC
        public OrderTypeEnum dir { get; set; }
    }


    //for jQuery datatables 
    public class DtSearchDto
    {
        //search value
        public string value { get; set; }

        //no used now
        public bool regex { get; set; }
    }
}

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
        public int draw { get; set; } = 0;

        //start row position, base 0
        public int start { get; set; } = 0;

        //rows count to display
        public int length { get; set; } = 0;

        //-1: re-count filtered rows count(means change jquery condition)
        public int recordsFiltered { get; set; } = 0;

        //query condition json in string type
        public string findJson { get; set; } = "";

        //優先考慮 sort 欄位, 內容為: A/D + fid, ex:Au.Account
        public string sort { get; set; } = "";

        //search word
        public DtSearchDto? search { get; set; } = null;

        /// <summary>
        /// sorting, get from jQuery DataTables, then get OrderBy, OrderDir
        /// </summary>
        public List<DtOrderDto>? order { get; set; } = null;

    }


    //for jQuery datatables 
    public class DtOrderDto
    {
        //sorting column index, base 0
        public int column { get; set; } = 0;

        //sorting column name
        public string fid { get; set; } = "";

        //sorting type: ASC(default), DESC
        public OrderTypeEnum dir { get; set; } = OrderTypeEnum.Asc;
    }


    //for jQuery datatables 
    public class DtSearchDto
    {
        //search value
        public string value { get; set; } = "";

        //no used now
        public bool regex { get; set; } = false;
    }
}

using System.Collections.Generic;
using Base.Enums;

namespace Base.Models
{
    /// <summary>
    /// for jQuery datatables input argument, use low camel
    /// </summary>
    public class DtDto
    {
        public int draw { get; set; }

        //start row position, base 0
        public int start { get; set; }

        //rows count to display
        public int length { get; set; }

        //quick search, need c# coding
        //public string search { get; set; }

        //-1: reCaculate rows count(means change jquery condition)
        public int recordsFiltered { get; set; }

        //query condition
        public string findJson { get; set; }

        //search word
        public DtSearchDto search { get; set; }

        /// <summary>
        /// sorting, get from jQuery DataTables, then get OrderBy, OrderDir
        /// </summary>
        public List<DtOrderDto> order { get; set; }

        /*
        /// <summary>
        /// 排序行數, -1表示無傳入
        /// </summary>
        public int orderColumn
        {
            get
            {
                return (order == null || order.Count == 0) ? -1 : order[0].column;
            }
        }

        /// <summary>
        /// 排序模式
        /// </summary>
        public EnumOrderDir orderDir
        {
            get
            {
                return (order == null || order.Count == 0) ? EnumOrderDir.Asc : order[0].dir;
            }
        }
        */
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

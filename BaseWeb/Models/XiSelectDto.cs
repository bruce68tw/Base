using Base.Models;
using System.Collections.Generic;

namespace BaseWeb.Models
{
    public class XiSelectDto : XiBaseDto
    {
        /*
        public XiSelectDto()
        {
            AddEmptyRow = true;
        }
        */

        public List<IdStrDto>? Rows { get; set; }

        //AddEmptyRow -> AddEmpty
        public bool AddEmpty { get; set; } = true;

        //move to XiBaseDto
        //public string FnOnChange { get; set; } = "";
    }
}
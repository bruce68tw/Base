using Base.Models;
using System.Collections.Generic;

namespace BaseWeb.Models
{
    public class XiSelectDto : XiBaseDto
    {
        public XiSelectDto()
        {
            AddEmptyRow = true;
        }

        public List<IdStrDto> Rows { get; set; }
        public bool AddEmptyRow { get; set; }
        public string FnOnChange { get; set; }
    }
}
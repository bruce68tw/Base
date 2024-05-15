using Base.Models;
using System.Collections.Generic;

namespace BaseWeb.Models
{
    public class XiRadioDto : XiBaseDto
    {
        /*
        public XiRadioDto()
        {
            IsHori = true;
        }
        */

        public List<IdStrDto> Rows { get; set; } = null!;
        public bool IsHori { get; set; } = true;
        public string FnOnChange { get; set; } = "";
    }
}
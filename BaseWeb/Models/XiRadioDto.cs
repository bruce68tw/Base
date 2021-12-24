using Base.Models;
using System.Collections.Generic;

namespace BaseWeb.Models
{
    public class XiRadioDto : XiBaseDto
    {
        public XiRadioDto()
        {
            IsHori = true;
        }

        public List<IdStrDto> Rows { get; set; }
        public bool IsHori { get; set; }
        public string FnOnChange { get; set; }
    }
}
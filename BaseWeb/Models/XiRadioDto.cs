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

        //FnOnChange -> FnOnClick(實際使用 onclick 事件)
        public string FnOnClick { get; set; } = "";
    }
}
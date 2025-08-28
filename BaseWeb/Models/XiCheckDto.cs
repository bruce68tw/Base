﻿namespace BaseWeb.Models
{
    public class XiCheckDto : XiBaseDto
    {
        /*
        public XiCheckDto()
        {
            IsCheck = false;
            Label = "";
            FnOnClick = "";
        }
        */

        public bool IsCheck { get; set; } = false;
        public string Label { get; set; } = "";
        public string FnOnClick { get; set; } = "";
    }
}
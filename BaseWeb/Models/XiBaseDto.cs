﻿namespace BaseWeb.Models
{
    public class XiBaseDto
    {
        /*
        public XiBaseDto()
        {
            //Edit = "";
            Value = "";
            InRow = false;
            //LabelTip = "";
            //InputTip = "";
            //ExtAttr = "";
            //ExtClass = "";
            Width = "100%";
        }
        */

        public string Title { get; set; } = "";
        public string Fid { get; set; } = "";
        public string Value { get; set; } = "";

        /// <summary>
        /// C(create), U(update), Other for none
        /// </summary>
        public string Edit { get; set; } = "";
        public bool InRow  { get; set; }
        public bool Required  { get; set; }
        public string LabelTip  { get; set; } = "";
        public string InputTip  { get; set; } = "";
        public string InputAttr  { get; set; } = "";
        public string BoxClass  { get; set; } = "";
        public string Cols { get; set; } = "";
        public string Width { get; set; } = "100%";
    }
}
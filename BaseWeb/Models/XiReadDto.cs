﻿namespace BaseWeb.Models
{
    public class XiReadDto
    {
        public XiReadDto()
        {
            InRow = false;
            SaveDb = false; //default not save Db !!
            EditStyle = false;
        }

        public string Title { get; set; } = "";
        public string Fid { get; set; } = "";
        public string Value { get; set; } = "";
        public bool InRow  { get; set; }
        public string LabelTip  { get; set; } = "";
        public string InputAttr  { get; set; } = "";
        public string BoxClass  { get; set; } = "";
        public string Cols { get; set; } = "";
        public string Format { get; set; } = "";

        /// <summary>
        /// 是否儲存DB, default false
        /// </summary>
        public bool SaveDb { get; set; }

        /// <summary>
        /// edit field style, default false
        /// </summary>
        public bool EditStyle { get; set; }
    }
}
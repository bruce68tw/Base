﻿namespace Base.Models
{
    /// <summary>
    /// locale resource for base class
    /// 調用 _Json.CopyModel(), 必須宣告為類別屬性 !!
    /// </summary>
    public class BaseResDto
    {
        //public string Locale { get; set; }

        //for view component
        public string BtnCreate { get; set; } = "";
        public string BtnAddRow { get; set; } = "";
        public string BtnFind { get; set; } = "";
        public string BtnFind2 { get; set; } = "";
        public string BtnSave { get; set; } = "";
        public string BtnToRead { get; set; } = "";
        public string BtnExport { get; set; } = "";
        public string BtnOpen { get; set; } = "";
        public string BtnReset { get; set; } = "";
        public string BtnYes { get; set; } = "";
        public string BtnCancel { get; set; } = "";
        public string BtnClose { get; set; } = "";

        //select input
        public string PlsSelect { get; set; } = "";

        //tip delete row
        public string TipDeleteRow { get; set; } = "";

        //no access right for program
        //public string NoAuthProg { get; set; } = "";

        //no auth for row
        //public string NoAuthRow { get; set; }

        //not login
        //public string NotLogin { get; set; } = "";

        //validation
        public string Required { get; set; } = "";
        public string StrLen { get; set; } = "";

        public string TipUpdate { get; set; } = "";
        public string TipDelete { get; set; } = "";
        public string TipView { get; set; } = "";
    }
}

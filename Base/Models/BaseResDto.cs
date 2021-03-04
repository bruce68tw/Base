namespace Base.Models
{
    /// <summary>
    /// locale resource for base class
    /// </summary>
    public class BaseResDto
    {
        //constructor
        public BaseResDto()
        {
            PlsSelect = "-Select-";
            //FrontDateFormat = "yyyy/M/d";
            //FrontDtFormat = "yyyy/M/d HH:mm:ss";   //24hour
        }

        public string Locale { get; set; }

        //for view component
        public string BtnCreate { get; set; }
        public string BtnAddRow { get; set; }
        public string BtnFind { get; set; }
        public string BtnFind2 { get; set; }
        public string BtnSave { get; set; }
        public string BtnToRead { get; set; }
        public string BtnExport { get; set; }
        public string BtnOpen { get; set; }
        public string BtnReset { get; set; }
        public string BtnYes { get; set; }
        public string BtnCancel { get; set; }
        public string BtnClose { get; set; }

        //select input
        public string PlsSelect { get; set; }

        //tip delete row
        public string TipDeleteRow { get; set; }

        //for sql read datetime column to front
        //public string FrontDateFormat { get; set; }
        //public string FrontDtFormat { get; set; }

        //no access right for program
        public string NoProgAuth { get; set; }

        //not login
        public string NotLogin { get; set; }
    }
}

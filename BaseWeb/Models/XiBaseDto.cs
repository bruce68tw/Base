namespace BaseWeb.Models
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

        /// <summary>
        /// 如果true, 則不會加上外層 row
        /// </summary>
        public bool InRow  { get; set; }

        public bool Required  { get; set; }
        public string LabelTip  { get; set; } = "";
        public string InputTip  { get; set; } = "";
        public string InputAttr  { get; set; } = "";

        /// <summary>
        /// 單一欄位在加在本身tag, 複合欄位會加在外層div
        /// </summary>
        public string BoxClass  { get; set; } = "";

        public string Cols { get; set; } = "";

        /// <summary>
        /// form-control預設100%, 如果不是則系統會設定為inline-block
        /// </summary>
        public string Width { get; set; } = "100%";
    }
}
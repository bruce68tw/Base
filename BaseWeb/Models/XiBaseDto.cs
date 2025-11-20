namespace BaseWeb.Models
{
    public class XiBaseDto
    {
        public string Title { get; set; } = "";

        //LabelTip -> TitleTip
        public string TitleTip { get; set; } = "";
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

        //placeholder
        public string InputTip  { get; set; } = "";

        public string InputAttr  { get; set; } = "";

        //欄位右方提示文字(.x-input-note)
        public string InputNote { get; set; } = "";

        /// <summary>
        /// css class, 如果單一欄位在加在本身tag, 複合欄位會加在外層div
        /// </summary>
        public string ClsBox  { get; set; } = "";

        public string Cols { get; set; } = "";

        /// <summary>
        /// 停用 inline style for CSRF !!
        /// form-control預設100%, 如果不是則系統會設定為inline-block
        /// </summary>
        public int Width { get; set; } = 0;

        //onclick, onchange event 傳入參數, 多個參數時用逗號分隔
        public string EventArgs { get; set; } = "";
    }
}
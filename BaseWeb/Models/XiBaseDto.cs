namespace BaseWeb.Models
{
    /// <summary>
    /// 所有 Xi...Dto.cs 的基底類別，在這裡宣告共用的欄位
    /// </summary>
    public class XiBaseDto
    {
        /// <summary>
        /// 欄位的標題文字
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// 標題的提示文字，如果有值則標題右側會出現(i)小圖示
        /// </summary>
        public string TitleTip { get; set; } = "";

        /// <summary>
        /// 欄位Id，通常會對應後端資料表欄位名稱，前端HTML會包裝成data-fid、name屬性
        /// </summary>
        public string Fid { get; set; } = "";

        /// <summary>
        /// 一般輸入欄位的初始值, 如果是checkbox/radio則是要存入DB的值
        /// </summary>
        public string Value { get; set; } = "";

        /// <summary>
        /// 欄位的編輯狀態，C表示只有新增時可編輯, U表示只有修改時可編輯, 空白表示不設限，
        /// 如果希望某欄位不可編輯則將此欄位設為不等於CU的任意字元, 例如:x
        /// </summary>
        public string Edit { get; set; } = "";

        /// <summary>
        /// 欄位是否在row css class裡面，如果true, 則不會再加上 row css class的 div外層
        /// </summary>
        public bool InRow  { get; set; }

        /// <summary>
        /// 是否為必填，如果是則會自動加上紅色星號
        /// </summary>
        public bool Required  { get; set; }

        /// <summary>
        /// 輸入欄位的提示文字，會轉換成input的 PlaceHolder 屬性
        /// </summary>
        public string InputTip  { get; set; } = "";

        /// <summary>
        /// 要加入的額外 input 屬性
        /// </summary>
        public string InputAttr  { get; set; } = "";

        /// <summary>
        /// 欄位右方的提示文字，會套用 .x-input-note css class, 如果是多行欄位(XiTextarea)則此文字會顯示在label(Title)下方!!
        /// </summary>
        public string InputNote { get; set; } = "";

        /// <summary>
        /// 額外加入的 css class, 如果input外面有包一層div，則此 css class 會加在外層div；否則會加在 input 上面
        /// </summary>
        public string ClsBox  { get; set; } = "";

        /// <summary>
        /// 排版用途，如果Title和input在水平位置，則此欄位會包含2個數字，例如 "2,3"，表示Title和input的欄位grid數(col-md-x 裡面的x)，
        /// 如果Title和input在垂直位置，則此欄位為1個數字，表示Title和input的欄位grid數(col-md-x 裡面的x)，
        /// 如果輸入欄位的Title為空，則Cols欄位無作用。
        /// </summary>
        public string Cols { get; set; } = "";

        /// <summary>
        /// 此欄位的用途是停用 inline style 以防止 CSP 警示 !!
        /// 例如指定100時，則會在input部分加上 .x-w100 css class 來設定欄位寬度，同時設定為inline-block
        /// 如果沒有指定(等於0), 則form-control預設寬度為100%, 
        /// </summary>
        public int Width { get; set; } = 0;

        /// <summary>
        /// 此欄位用途是避免使用 inline script 以防止 CSP 警示
        /// 內容是 onclick, onchange event 的傳入參數, 多個參數時用逗號分隔,
        /// 系統會將此欄位值寫入 data-args 屬性
        /// </summary>
        public string EventArgs { get; set; } = "";

        /// <summary>
        /// 此欄位用途是避免使用 inline script 以防止 CSP 警示
        /// 目前會觸發 onchange 事件的輸入欄位有：select、text、check、radio、date
        /// 如果設定此欄位，則系統會將此欄位值寫入 data-onchange 屬性
        /// </summary>
        public string FnOnChange { get; set; } = "";

    }
}
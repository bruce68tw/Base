namespace BaseWeb.Models
{
    //不繼承 XiBaseDto !!
    public class XiCheckTextDto
    {
        /// <summary>
        /// 欄位Id，通常會對應後端資料表欄位名稱，前端HTML會包裝成data-fid、name屬性
        /// </summary>
        public string Fid { get; set; } = "";

        /// <summary>
        /// Checkbox後面的說明文字
        /// </summary>
        public string Label { get; set; } = "";

        /// <summary>
        /// 欄位的編輯狀態，C表示只有新增時可編輯, U表示只有修改時可編輯, 空白表示不設限，
        /// 如果希望某欄位不可編輯則將此欄位設為不等於CU的任意字元, 例如:x
        /// </summary>
        public string Edit { get; set; } = "";

        /// <summary>
        /// 額外加入的 css class, 如果input外面有包一層div，則此 css class 會加在外層div；否則會加在 input 上面
        /// </summary>
        public string TextClsBox { get; set; } = "";

        /// <summary>
        /// 欄位的最大長度，0表示不限制
        /// </summary>
        public int TextMaxLen { get; set; } = 0;

        /// <summary>
        /// 此欄位的用途是停用 inline style 以防止 CSP 警示 !!
        /// 例如指定100時，則會在input部分加上 .x-w100 css class 來設定欄位寬度，同時設定為inline-block
        /// 如果沒有指定(等於0), 則form-control預設寬度為100%, 
        /// </summary>
        //public int TextWidth { get; set; } = 0;

    }
}
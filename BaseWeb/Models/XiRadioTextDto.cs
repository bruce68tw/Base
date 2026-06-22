namespace BaseWeb.Models
{
    //繼承 XiRadioDto，增加 TextMaxLen 與 TextClsBox 兩個屬性
    public class XiRadioTextDto : XiRadioDto
    {
        /// <summary>
        /// 額外加入的 css class, 如果input外面有包一層div，則此 css class 會加在外層div；否則會加在 input 上面
        /// </summary>
        public string TextClsBox { get; set; } = "";

        /// <summary>
        /// 欄位的最大長度，0表示不限制
        /// </summary>
        public int TextMaxLen { get; set; } = 0;

    }
}
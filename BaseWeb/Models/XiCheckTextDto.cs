namespace BaseWeb.Models
{
    //繼承 XiCheckDto !!
    public class XiCheckTextDto : XiCheckDto
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
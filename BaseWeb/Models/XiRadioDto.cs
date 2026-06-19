using Base.Models;
using System.Collections.Generic;

namespace BaseWeb.Models
{
    /// <summary>
    /// XiRadio 輸入欄位的傳入參數類型，Radio欄位為多選一，一個Radio欄位會包含多個 Radio按鈕
    /// </summary>
    public class XiRadioDto : XiBaseDto
    {
        /// <summary>
        /// 會展開成多個Radio按鈕，資料類型為IdStrDto，其中model的Id表示Radio Value，Str為顯示文字
        /// </summary>
        public List<IdStrDto>? Rows { get; set; } = null!;

        /// <summary>
        /// 多個Radio按鈕預設為水平排列
        /// </summary>
        public bool IsHori { get; set; } = true;
    }
}
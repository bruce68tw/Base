using Base.Models;
using System.Collections.Generic;

namespace BaseWeb.Models
{
    /// <summary>
    /// XiSelect 下拉式輸入欄位的傳入參數類型
    /// </summary>
    public class XiSelectDto : XiBaseDto
    {

        /// <summary>
        /// select options 的項目清單，資料型態為 IdStrDto，其中 model 的 Id 為 Option Value，Str 為顯示文字
        /// </summary>
        public List<IdStrDto>? Rows { get; set; }

        /// <summary>
        /// 是否在 options 最上面增加一空白列，顯示文字通常是 "請選擇"
        /// </summary>
        public bool AddEmpty { get; set; } = true;
    }
}
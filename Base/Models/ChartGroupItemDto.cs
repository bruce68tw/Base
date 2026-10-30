using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// 分組圖表中單一系列的標籤、顏色與整數資料。
    /// </summary>
    public class ChartGroupItemDto
    {
        /// <summary>
        /// 系列名稱。
        /// </summary>
        public string label { get; set; } = "";
        /// <summary>
        /// 此系列共用的背景顏色。
        /// </summary>
        public string backgroundColor { get; set; } = "";
        /// <summary>
        /// 依共同標籤順序排列的數值，預設未初始化。
        /// </summary>
        public List<int> data { get; set; } = null!;
    }
}

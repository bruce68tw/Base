using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// 一般圖表的一組整數資料點及其顏色。
    /// </summary>
    public class ChartItemDto
    {
        //public string label { get; set; }
        /// <summary>
        /// 各資料點的顏色清單，預設未初始化。
        /// </summary>
        public List<string> backgroundColor { get; set; } = null!;
        /// <summary>
        /// 依標籤順序排列的整數值，預設未初始化。
        /// </summary>
        public List<int> data { get; set; } = null!;
    }
}

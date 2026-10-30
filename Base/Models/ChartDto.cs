using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// 一般統計圖資料，屬性命名配合 Chart.js。
    /// </summary>
    public class ChartDto
    {
        /// <summary>
        /// 圖表標題。
        /// </summary>
        public string title { get; set; } = "";
        /// <summary>
        /// 分類標籤，與資料點順序對應；預設未初始化。
        /// </summary>
        public List<string> labels { get; set; } = null!;
        /// <summary>
        /// 圖表資料集；預設未初始化。
        /// </summary>
        public List<ChartItemDto> datasets { get; set; } = null!;
    }

}

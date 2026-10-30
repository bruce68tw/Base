using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// 分組統計圖資料，屬性命名配合 Chart.js。
    /// </summary>
    public class ChartGroupDto
    {
        /// <summary>
        /// 圖表標題。
        /// </summary>
        public string title { get; set; } = "";
        /// <summary>
        /// 共同分類標籤；預設未初始化。
        /// </summary>
        public List<string> labels { get; set; } = null!;
        /// <summary>
        /// 各組資料集；預設未初始化。
        /// </summary>
        public List<ChartGroupItemDto> datasets { get; set; } = null!;
    }

}

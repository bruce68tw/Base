using System.Collections.Generic;

namespace Base.Models
{
    //for output group bar chart, 欄位名稱和大小寫配合 Chart.js
    public class ChartGroupItemDto
    {
        public string label { get; set; }

        public string backgroundColor { get; set; }

        public List<int> data { get; set; }
    }
}
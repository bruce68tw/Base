using System.Collections.Generic;

namespace Base.Models
{
    //for output group bar chart, 欄位名稱和大小寫配合 Chart.js
    public class ChartItemDto
    {
        //public string label { get; set; }

        public List<string> backgroundColor { get; set; }

        public List<int> data { get; set; }
    }
}
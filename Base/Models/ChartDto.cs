using System.Collections.Generic;

namespace Base.Models
{
    //欄位名稱和大小寫配合 Chart.js
    public class ChartDto
    {
        public string title { get; set; } = "";

        //label list
        public List<string> labels { get; set; } = null!;

        public List<ChartItemDto> datasets { get; set; } = null!;
    }

}
using System.Collections.Generic;

namespace Base.Models
{
    //欄位名稱和大小寫配合 Chart.js
    public class ChartGroupDto
    {
        public string title { get; set; }

        //label list
        public List<string> labels { get; set; }

        public List<ChartGroupItemDto> datasets { get; set; }
    }

}
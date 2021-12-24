using System;

namespace Base.Models
{
    //https://www.helloweba.com/view-blog-231.html
    public class CalendarEventDto
    {
        public string id { get; set; }
        public string title { get; set; }
        public bool allDay { get; set; }
        public DateTime startDT { get; set; }
        public DateTime endDT { get; set; }
        public string start
        {
            get
            {
                return startDT.ToString("s");
            }
        }
        public string end
        {
            get
            {
                return endDT.ToString("s");
            }
        }

        public string color { get; set; }
        public string textColor { get; set; }

        /*
        public string url
        {
            get
            {
                return "/Calendar/Detail/" + id;
            }
        }
        */

    }//class
}

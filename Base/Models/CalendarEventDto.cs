using System;

namespace Base.Models
{
    /// <summary>
    /// 行事曆事件資料；start 與 end 由日期時間屬性即時計算。
    /// </summary>
    public class CalendarEventDto
    {
        /// <summary>
        /// 事件識別碼。
        /// </summary>
        public string id { get; set; } = "";
        /// <summary>
        /// 事件標題。
        /// </summary>
        public string title { get; set; } = "";
        /// <summary>
        /// 是否為全天事件。
        /// </summary>
        public bool allDay { get; set; }
        /// <summary>
        /// 事件開始時間。
        /// </summary>
        public DateTime startDT { get; set; }
        /// <summary>
        /// 事件結束時間。
        /// </summary>
        public DateTime endDT { get; set; }
        /// <summary>
        /// 開始時間的 yyyy-MM-ddTHH:mm:ss 字串，不附時區。
        /// </summary>
        public string start
        {
            get
            {
                return startDT.ToString("s");
            }
        }
        /// <summary>
        /// 結束時間的 yyyy-MM-ddTHH:mm:ss 字串，不附時區。
        /// </summary>
        public string end
        {
            get
            {
                return endDT.ToString("s");
            }
        }
        /// <summary>
        /// 事件背景顏色字串。
        /// </summary>
        public string color { get; set; } = "";
        /// <summary>
        /// 事件文字顏色字串。
        /// </summary>
        public string textColor { get; set; } = "";

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

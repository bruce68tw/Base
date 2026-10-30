using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// 簡易分頁結果容器；資料列可為 null。
    /// </summary>
    public class PageOut<T>
    {
        /*
        public PageOut()
        {
            Rows = new List<T>();
        }
        */
        /// <summary>
        /// 分頁資訊的 JSON 字串，具體鍵名由產生端決定。
        /// </summary>
        public string PageArg { get; set; } = "";
        /*
        //public int draw { get; set; }
        public int page { get; set; }

        //condition rows count
        public int recordsFiltered { get; set; }
        */
        /// <summary>
        /// 本頁資料列，預設 null。
        /// </summary>
        public List<T>? Rows { get; set; }
        /// <summary>
        /// 錯誤訊息，空字串表示未設定錯誤。
        /// </summary>
        public string _ErrorMsg { get; set; } = "";
    }
}

using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// for RWD pagin output(jquery simple pagination)
    /// refer CrudRead.cs GetPageAsync() return json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageOut<T>
    {
        public PageOut()
        {
            Rows = new List<T>();
        }

        //json string has: pageNo, pageRows, filterRows
        public string PageArg { get; set; }
        /*
        //public int draw { get; set; }
        public int page { get; set; }

        //condition rows count
        public int recordsFiltered { get; set; }
        */

        public List<T> Rows { get; set; }

        /// <summary>
        /// error msg if any (necessary field for resultXXX dto)
        /// refer ResultDto.cs
        /// </summary>
        public string ErrorMsg { get; set; }
    }
}
using Base.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Base.Services
{
    /// <summary>
    /// for EasyDtDto
    /// </summary>
    public class _Page
    {
        //private static List<int> PageRowRanges = new() { 10, 25, 50, 100 };
        private static List<int> PageRowRanges = [10, 20, 50, 100]; //for more friendly !!

        public static PageIn GetPageIn(int pageNo, int pageRows, int filterRows, List<object>? args = null)
        {
            //pageRows = GetPageRows(pageRows);
            var result = new PageIn() 
            { 
                //start = (pageNo - 1) * pageRows,
                page = pageNo,
                length = pageRows,
                filterRows = filterRows,
            };
            if (args != null)
            {
                var json = new JObject();
                for (var i=0; i<args.Count; i+=2 )
                    json[args[i]] = args[i + 1].ToString();
                result.findJson = _Json.ToStr(json);
            }
            return result;
        }

        public static int GetPageRows(int pageRows)
        {
            return PageRowRanges.Contains(pageRows)
                ? pageRows : PageRowRanges[0];
        }

        public static PageOut<T> GetError<T>(PageOut<T> page, string error = "") where T : class
        {
            page.ErrorMsg = _Str.EmptyToValue(error, _Fun.SystemError);
            return page;
        }

        public static PageOut<T> GetBrError<T>(PageOut<T> page, string fid) where T : class
        {
            page.ErrorMsg = _Fun.PreBrError + fid;
            return page;
        }

        /*
        public static void SetPageArg<T>(PageOut<T> page, int pageNo, int pageRows, int filterCount) where T : class
        {
            var json = JObject.FromObject(new
            {
                page = pageNo,
                filterCount = filterCount,
                pageRows = pageRows,
            });
            page.PageArg = _Json.ToStr(json);
        }
        */
    }
}

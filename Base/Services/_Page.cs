using Base.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Base.Services
{
    /// <summary>
    /// 簡易分頁輸入與錯誤結果的輔助工具。
    /// </summary>
    public class _Page
    {
        //private static List<int> PageRowRanges = new() { 10, 25, 50, 100 };
        private static List<int> LengthMenu = [10, 20, 50, 100, 150]; //for more friendly !!

        /// <summary>
        /// 建立頁碼與筆數設定；args 為名稱、值交錯清單，轉入 findJson。
        /// </summary>
        /// <param name="pageNo">要讀取的頁碼</param>
        /// <param name="pageRows">每頁資料筆數</param>
        /// <param name="filterRows">已知的篩選後資料筆數</param>
        /// <param name="args">查詢欄位名稱與值交錯排列的清單。</param>
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

        /// <summary>
        /// 將筆數限制為 10、20、50、100、150，其他值改用 10。
        /// </summary>
        /// <param name="pageRows">每頁資料筆數</param>
        public static int GetPageRows(int pageRows)
        {
            return LengthMenu.Contains(pageRows)
                ? pageRows : LengthMenu[0];
        }

        /// <summary>
        /// 原地設定分頁錯誤訊息，空訊息改用系統錯誤文字。
        /// </summary>
        /// <param name="page">要原地設定錯誤資訊的分頁結果</param>
        /// <param name="error">要記錄或回傳的錯誤訊息</param>
        public static PageOut<T> GetError<T>(PageOut<T> page, string error = "") where T : class
        {
            page._ErrorMsg = _Str.EmptyToValue(error, _Fun.SystemError);
            return page;
        }

        /// <summary>
        /// 原地設定帶有 B: 前綴的語系錯誤鍵。
        /// </summary>
        /// <param name="page">要原地設定錯誤資訊的分頁結果</param>
        /// <param name="fid">要處理的欄位識別值</param>
        public static PageOut<T> GetBrError<T>(PageOut<T> page, string fid) where T : class
        {
            page._ErrorMsg = _Fun.PreBrError + fid;
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

using Base.Models;
using System.Collections.Generic;

namespace Base.Services
{
    /// <summary>
    /// 陣列字串組合與選項資料轉換。
    /// </summary>
    public class _Array
    {
        /// <summary>
        /// 以 sep 連接陣列，可為各項加單引號；不進行 SQL 跳脫。
        /// </summary>
        public static string ToStr(string[] list, bool quote = false, string sep = ",")
        {
            //return (list.Length == 0)
            //    ? "" : string.Join(sep, list);
            var result = (list == null || list.Length == 0)
                ? "" : string.Join(sep, list);
            return (quote)
                ? "'" + result.Replace(sep, "'" + sep + "'") + "'"
                : result;
        }

        /// <summary>
        /// 將 Id、顯示文字交錯的偶數個參數轉為選項清單。
        /// </summary>
        public static List<IdStrDto> ToIdStrs(params string[] args)
        {
            var data = new List<IdStrDto>();
            for (var i=0; i<args.Length; i+=2)
            {
                data.Add(new IdStrDto()
                {
                    Id = args[i],
                    Str = args[i + 1],
                });
            }
            return data;
        }

    }//class
}

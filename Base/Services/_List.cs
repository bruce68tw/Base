using Base.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    public class _List
    {
        /// <summary>
        /// convert list<string> to string
        /// </summary>
        /// <param name="list"></param>
        /// <param name="quote">是否加上單引號</param>
        /// <param name="sep"></param>
        /// <returns></returns>
        public static string ToStr(List<string> list, bool quote = false, string sep = ",")
        {
            var result = (list == null || list.Count == 0)
                ? "" : string.Join(sep, list);
            return (quote)
                ? "'" + result.Replace(sep, "'" + sep + "'") + "'"
                : result;
        }

        //overloading
        public static string ToStr(List<int> list, bool quote, string sep = ",")
        {
            var result = (list == null || list.Count == 0)
                ? "" : string.Join(sep, list);
            return (quote)
                ? "'" + result.Replace(sep, "'" + sep + "'") + "'"
                : result;
        }

        /// <summary>
        /// check key with rules
        /// </summary>
        /// <param name="list"></param>
        /// <param name="logError"></param>
        /// <returns></returns>
        public static bool CheckKey(List<string> list, bool logError = true)
        {
            return _Str.CheckKey(ToStr(list), logError);
        }

        public static List<string>? Concat(List<string?>? list1, List<string>? list2)
        {
            var list = new List<string>();
            if (list1 != null && list1.Count > 0)
                list.AddRange(list1!);
            if (list2 != null && list2.Count > 0)
                list.AddRange(list2);
            return (list.Count == 0) ? null : list;
        }

        public static List<IdStrDto> CodesAddEmpty(List<IdStrDto>? codes, string plsSelect)
        {
            codes ??= [];
            codes.Insert(0, new IdStrDto()
            {
                Id = "",
                Str = plsSelect,
            });
            return codes;
        }

        public static string? StrToId(List<IdStrDto>? codes, string str)
        {
            if (codes == null || codes.Count == 0)
                return null;

            return codes.Where(a => a.Str == str)
                .Select(a => a.Id)
                .FirstOrDefault();
        }

        public static bool IsEmpty<T>(List<T>? rows) where T : class
        {
            return rows == null || rows.Count == 0;
        }
        public static bool NotEmpty<T>(List<T>? rows) where T : class
        {
            return !IsEmpty(rows);
        }
    }//class
}

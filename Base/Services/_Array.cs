using Base.Models;
using System.Collections.Generic;

namespace Base.Services
{
    public class _Array
    {
        //string[] to string 
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
        /// convert string array to IdStrDto list
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
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

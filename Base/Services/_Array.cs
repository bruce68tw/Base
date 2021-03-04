using Base.Models;
using System.Collections.Generic;

namespace Base.Services
{
    public class _Array
    {
        //string[] to string 
        public static string ToStr(string[] list, string sep = ",")
        {
            return (list == null || list.Length == 0)
                ? ""
                : string.Join(sep, list);
        }

        /// <summary>
        /// convert string array to IdStrDto list
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static List<IdStrDto> ToIdStrs(params string[] args)
        {
            var data = new List<IdStrDto>();
            for (var i=0; i<args.Length; i=i+2)
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

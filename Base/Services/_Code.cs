using Base.Models;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// 處理 XpCode table
    /// </summary>
    public class _Code
    {
        //filter json array
        public static JArray? FilterArray(JArray rows, string fid, string value)
        {
            //if (rows == null) return null;

            var finds = new JArray();
            foreach (var row in rows)
                if (row[fid]!.ToString() == value) finds.Add(row);
            return (finds.Count == 0)
                ? null : finds;
        }

        public static List<IdStrDto>? FilterRows(List<IdStrExtDto>? rows, string value)
        {
            return (rows == null || rows.Count == 0)
                ? null
                : rows.Where(a => a.Ext == value)
                    .Select(a => new IdStrDto { Id = a.Id, Str = a.Str })
                    .ToList();
        }

        /// <summary>
    }//class
}

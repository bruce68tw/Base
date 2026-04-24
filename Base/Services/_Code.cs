using Base.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Base.Services
{
    /// <summary>
    /// 處理 XpCode table
    /// </summary>
    public class _Code
    {
        //FilterArray -> FilterJsons
        //filter json array
        public static JArray? FilterJsons(JArray rows, string fid, string value)
        {
            //if (rows == null) return null;

            var finds = new JArray();
            foreach (var row in rows)
                if (row[fid]!.ToString() == value) finds.Add(row);
            return (finds.Count == 0)
                ? null : finds;
        }

        //FilterRows -> FilterList
        public static List<IdStrDto>? FilterList(List<IdStrExtDto>? rows, string value)
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

using Base.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    public static class _Chart
    {
        //彩虹顏色
        private static List<string> _colors = new() {
            "#F32E37",
            "#EABE37",
            //"#89E926",
            "#22E352",
            "#2FE5E8",
            "#295AE7",
            "#8828EE",
            "#E629B7",
        };

        /// <summary>
        /// for 一般統計圖
        /// </summary>
        /// <param name="title"></param>
        /// <param name="sql"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static async Task<ChartDto> GetDataA(string title, string sql, Db db = null)
        {
            //read db rows & close
            var newDb = _Db.CheckOpenDb(ref db);
            var rows = await _Db.GetModelsA<IdNumDto>(sql, null, db);
            await _Db.CheckCloseDbA(db, newDb);

            //initial result
            var result = new ChartDto
            {
                title = title,
                //labels = labels,
                datasets = new(),  //new Dictionary<string, List<int>>(),
            };
            if (rows == null)
                return result;

            //get labels
            result.labels = rows.Select(a => a.Id).ToList();
            result.datasets.Add(new(){ 
                backgroundColor = _colors.Take(result.labels.Count).ToList(),
                data = rows.Select(a => a.Num).ToList()
            });
            return result;
        }

        /// <summary>
        /// get data for group bar chart
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="labels">x座標label清單</param>
        /// <param name="colFids">欄位Id清單,空白表示同labels</param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static async Task<ChartGroupDto> GetGroupDataA(string title, string sql, List<string> labels, List<string> colFids = null, Db db = null)
        {
            //read db rows & close
            var newDb = _Db.CheckOpenDb(ref db);
            var rows = await _Db.GetModelsA<RowColNumDto>(sql, null, db);
            await _Db.CheckCloseDbA(db, newDb);

            //set colNameMap
            var colLen = labels.Count;
            colFids ??= labels;
            var colNameMap = new Dictionary<string, int>();
            for (var i=0; i< colLen; i++)
                colNameMap.Add(colFids[i], i);

            //initial result
            var result = new ChartGroupDto
            {
                title = title,
                labels = labels,
                datasets = new(),  //new Dictionary<string, List<int>>(),
            };
            if (rows == null)
                return result;

            //convert vertical rows into horizontal result
            foreach (var row in rows)
            {
                var rowName = row.Row;
                var findRow = result.datasets.FirstOrDefault(a => a.label == rowName);
                if (findRow == null)
                {
                    findRow = new()
                    {
                        label = rowName,
                        backgroundColor = _colors[result.datasets.Count],
                        data = new(new int[colLen]),   //同時設定資料欄位數
                    };
                    result.datasets.Add(findRow);
                }
                var colIdx = colNameMap.FirstOrDefault(a => a.Key == row.Col).Value;
                findRow.data[colIdx] = row.Num;
            }
            return result;
        }

    }//class
}

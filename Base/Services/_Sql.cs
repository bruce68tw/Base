using Base.Enums;
using Base.Models;
using System.Collections.Generic;

namespace Base.Services
{
    public class _Sql
    {
        //convert sql string to model 
        public static SqlDto SqlToDto(string sql, bool useSquare)
        {
            if (string.IsNullOrEmpty(sql))
                return null;

            var sql2 = sql.ToLower();
            var len = sql2.Length;
            int from, where, group, order;
            if (useSquare)
            {
                from = sql2.IndexOf("[from]");
                where = sql2.IndexOf("[where]");
                group = sql2.IndexOf("[group]");
                order = sql2.IndexOf("[order]");
            }
            else
            {
                from = sql2.IndexOf("from ");
                where = sql2.IndexOf("where ");
                group = sql2.IndexOf("group ");
                order = sql2.IndexOf("order ");
            }

            var end = len;
            var result = new SqlDto();
            if (order < 0)
                result.Order = "";
            else
            {
                result.Order = useSquare ? "Order " + sql.Substring(order + 7).Trim() : sql.Substring(order).Trim();
                end = order - 1;
            }

            if (group < 0)
                result.Group = "";
            else
            {
                result.Group = useSquare ? "Group " + sql.Substring(group + 7 , end - group - 7).Trim() : sql.Substring(group, end - group).Trim();
                end = group - 1;
            }

            if (where < 0)
                result.Where = "";
            else
            {
                result.Where = useSquare ? "Where " + sql.Substring(where + 7, end - where - 7).Trim() : sql.Substring(where, end - where).Trim();
                end = where - 1;
            }

            if (useSquare)
            {
                result.From = "From " + sql.Substring(from + 6, end - from - 6).Trim();
                result.Select = sql.Substring(0, from).Trim().Substring(7);     //exclude "Select" word !!
            }
            else
            {
                result.From = sql.Substring(from, end - from).Trim();
                result.Select = sql.Substring(0, from).Trim().Substring(7);     //exclude "Select" word !!
            }

            //set columns[]
            result.Columns = result.Select.Split(',');
            for (var i=0; i<result.Columns.Length; i++)
            {
                var col = result.Columns[i].Trim();
                var pos = col.IndexOf(" ");
                result.Columns[i] = (pos > 0) ? col.Substring(0, pos) : col;
            }
            //result.Columns = result.Select.Replace(" ","").Split(',');
            return result;
        }

        public static string DtoToSql(SqlDto model)
        {
            var sql = "select " + model.Select + " " +
                model.From + " " +
                model.Where + " " +
                model.Group + " " +
                model.Order;
            return sql.Replace("  ", " ");
        }

        /// <summary>
        /// sql string add in condition
        /// </summary>
        /// <param name="fid">傳入參數欄位id</param>
        /// <param name="keys">in key值array</param>
        /// <param name="args">要回傳的sql傳入參數, by ref</param>
        /// <returns>in 字串</returns>    
        public static string SqlAddIn(string fid, string[] keys, ref List<object> args)
        {
            //args.Clear();     //不清空 !!
            fid = "_" + fid;    //加上底線, 避免和其他變數衝突
            var data = "";      //return data
            var sep = "";       //分隔符號
            //var fid2 = "";      //sql傳入參數欄位id
            for (var i=0; i<keys.Length; i++)
            {
                var fid2 = fid + i;
                data += sep + "@" + fid2;
                sep = ",";

                args.Add(fid2);
                args.Add(keys[i]);
            }

            return data;
        }

        /// <summary>
        /// get where condition by list string
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string GetCondByListStr(string fid, List<string> list)
        {
            return fid + " in (" + _List.ToStr(list, true) + ")";
        }

        /// <summary>
        /// 如果 sql 中有用到字串相加的功能(concat), 可以呼叫這個函數來轉換, 以適用其他資料庫種類.
        /// </summary>
        /// <param name="values">第後一個欄位為 alias 名稱, 可為空白</param>
        /// <returns>string</returns>
        //public static string sqlConcat(string ps_sql, params string[] pas_input)
        public static string GetConcat(params string[] values)
        {
            int len = values.Length;
            if (len <= 1)
                return "";

            string op;
            string sql = "";
            switch (_Fun.GetDbType())
            {
                case DbTypeEnum.MSSql:
                    op = " + ";
                    break;
                case DbTypeEnum.MySql:
                    op = ", ";
                    sql = "concat";
                    break;
                case DbTypeEnum.Oracle:
                    op = " || ";
                    break;
                default:
                    return "";
            }

            string list = "";
            for (int i = 0; i < len; i++)
                list += values[i] + op;

            return sql + "(" + list + ")";
        }

    }//class
}

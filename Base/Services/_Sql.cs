using Base.Enums;
using Base.Models;
using System.Collections.Generic;

namespace Base.Services
{
    /// <summary>
    /// SQL 區段、分頁與資料範圍條件組合；不是通用 SQL 解析器。
    /// </summary>
    public class _Sql
    {
        /// <summary>
        /// 依讀取權限產生 u.Id 或 u.DeptId 條件；無權限時產生 and 1=0。
        /// </summary>
        public static string GetCondByAuth(string ctrl, BaseUserDto? br = null)
        {
            br ??= _Fun.GetBaseUser();
            var range = _Auth.GetAuthRange(ctrl, CrudEnum.Read, br.ProgAuthStrs);
            return range switch
            {
                AuthRangeEnum.User => $" and u.Id='{br.UserId}'",
                AuthRangeEnum.Dept => $" and u.DeptId='{br.DeptId}'",
                AuthRangeEnum.All => "",
                _ => " and 1=0"
            };
        }

        /// <summary>
        /// 以關鍵字位置拆分 SELECT SQL，可改用方括號區段；不支援任意複雜 SQL 語法。
        /// </summary>
        /// <param name="sql">如果空白會傳回 empty SqlDto</param>
        /// <param name="useSquare"></param>
        /// <returns></returns>
        public static SqlDto SqlToDto(string sql, bool useSquare)
        {
            //if (_Str.IsEmpty(sql)) return null;

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
                result.Order = useSquare ? "Order " + sql[(order + 7)..].Trim() : sql[order..].Trim();
                end = order - 1;
            }

            if (group < 0)
                result.Group = "";
            else
            {
                result.Group = useSquare ? "Group " + sql[(group + 7)..end].Trim() : sql[group..end].Trim();
                end = group - 1;
            }

            if (where < 0)
                result.Where = "";
            else
            {
                result.Where = useSquare ? "Where " + sql[(where + 7)..end].Trim() : sql[where..end].Trim();
                end = where - 1;
            }

            if (useSquare)
            {
                result.From = "From " + sql[(from + 6)..end].Trim();
                result.Select = sql[..from].Trim()[7..];     //exclude "Select" word !!
            }
            else
            {
                result.From = sql[from..end].Trim();
                result.Select = sql[..from].Trim()[7..];     //exclude "Select" word !!
            }

            //set columns[]
            result.Columns = result.Select.Split(',');
            for (var i=0; i<result.Columns.Length; i++)
            {
                var col = result.Columns[i].Trim();
                var pos = col.IndexOf(' ');
                result.Columns[i] = (pos > 0) ? col[..pos] : col;
            }
            //result.Columns = result.Select.Replace(" ","").Split(',');
            return result;
        }

        /// <summary>
        /// 依初始化的 DB 分頁格式組合 SQL；length 不大於 0 時讀取全部且不加排序。
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="start"></param>
        /// <param name="length">0表示讀取全部資料, 並且不使用order by</param>
        /// <returns></returns>
        public static string DtoToSql(SqlDto dto, int start, int length)
        {
            var group = (dto.Group == "") ? "" : " " + dto.Group;
            var sql = dto.Select + " " +
                dto.From + " " +
                dto.Where + group;

            var sql2 = (length > 0)
                ? string.Format(_Fun.ReadPageSql, sql, dto.Order, start, length)
                : "select " + sql;
            return sql2.Replace("  ", " ");   //2012
        }

        /// <summary>
        /// 建立參數化 IN 的名稱清單，並將名稱與值追加至 args。
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
        /// 直接將清單加引號組成 IN 條件，不跳脫值或使用參數。
        /// </summary>
        public static string GetCondByListStr(string fid, List<string> list)
        {
            return fid + " in (" + _List.ToStr(list, true) + ")";
        }

        //public static string sqlConcat(string ps_sql, params string[] pas_input)
        /// <summary>
        /// 依資料庫種類組合 SQL 字串串接；少於兩項或種類未支援時回傳空字串。
        /// </summary>
        /// <param name="values">第後一個欄位為 alias 名稱, 可為空白</param>
        /// <returns>string</returns>
        //public static string sqlConcat(string ps_sql, params string[] pas_input)
        public static string GetConcat(params string[] values)
        {
            var len = values.Length;
            if (len <= 1) return "";

            string op;
            var sql = "";
            switch (_Fun.DbType)
            {
                case DbTypeEnum.MSSql:
                    op = " + "; break;
                case DbTypeEnum.MySql:
                    op = ", ";
                    sql = "concat"; break;
                case DbTypeEnum.Oracle:
                    op = " || "; break;
                default:
                    return "";
            }

            var list = "";
            for (var i = 0; i < len; i++)
                list += values[i] + op;
            return sql + "(" + list[..^op.Length] + ")";
        }

    }//class
}

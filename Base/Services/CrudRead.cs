using Base.Enums;
using Base.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Base.Services
{
    /// <summary>
    /// for Crud Read Service
    /// </summary>
    public class CrudRead
    {
        //search field name for sql args
        private const string _FindId = "_find";

        //db str in config file
        private string _dbStr;

        //jQuery dataTables input arg
        private DtDto _dtIn;

        //sql args, (id, value)
        private List<object> _sqlArgs = new List<object>();

        //constructor
        public CrudRead(string dbStr = "")
        {
            _dbStr = dbStr;
        }

        private Db GetDb()
        {
            return new Db(_dbStr);
        }

        /// <summary>
        /// get page rows for dataTables(json)
        /// </summary>
        /// <param name="crud"></param>
        /// <param name="dt"></param>
        /// <param name="findJson">if not null, will query by this and not from dtIn.findJson</param>
        /// <returns>jquery dataTables object</returns>
        public JObject GetPage(ReadDto crud, DtDto dt, JObject findJson = null)
        {
            //adjust
            if (dt.length < 10)
                dt.length = 10;

            //set instance variables from input args
            _dtIn = dt;
            if (findJson == null)
                findJson = string.IsNullOrEmpty(dt.findJson) ? null : _Json.StrToJson(dt.findJson);

            //convert sql to model
            var sqlDto = _Sql.SqlToDto(crud.ReadSql, crud.UseSquare);
            if (sqlDto == null)
                return null;

            //prepare sql where & set sql args by user input condition
            //var sqlModel = sqlModel0.Value;
            var where = GetWhere(crud, findJson, _dtIn.search.value);
            if (where != "")
                sqlDto.Where = (sqlDto.Where == "") 
                    ? "Where " + where : sqlDto.Where + " And " + where;

            #region get rows count if need
            JArray rows = null;
            var db = GetDb();
            var rowCount = dt.recordsFiltered;
            var sql = "";
            var group = (sqlDto.Group == "") ? "" : " " + sqlDto.Group; //remove last space
            if (rowCount < 0)
            {
                sql = "Select Count(*) as _count " +
                    sqlDto.From + " " +
                    sqlDto.Where +
                    group;
                var row = db.GetJson("\n" + sql, _sqlArgs); //for log 斷行
                if (row == null)
                {
                    rowCount = 0;
                    goto lab_exit;
                }

                //case of ok
                rowCount = Convert.ToInt32(row["_count"]);
            }
            #endregion

            //sorting
            var orderColumn = (dt.order == null || dt.order.Count == 0) 
                ? -1 : dt.order[0].column;
            if (orderColumn >= 0)
                sqlDto.Order = "Order By " + 
                    sqlDto.Columns[orderColumn].Trim() + 
                    (dt.order[0].dir == OrderTypeEnum.Asc ? "" : " Desc");

            //set dbModel, consider crud.ColList
            sql = sqlDto.Select + " " + 
                sqlDto.From + " " + 
                sqlDto.Where + group;

            //get data
            sql = string.Format(_Fun.ReadPageSql, sql, sqlDto.Order, dt.start, dt.length).Replace("  ", " ");   //2012
            rows = db.GetJsons(sql, _sqlArgs);

            lab_exit:
            var result = JObject.FromObject(new
            {
                dt.draw,
                data = rows,
                recordsTotal = 0,
                recordsFiltered = rowCount,
            });

            //close db & return
            db.Dispose();
            return result;
        }

        /// <summary>
        /// get sql args
        /// </summary>
        /// <returns></returns>
        public List<object> GetArgs()
        {
            return _sqlArgs;
        }

        //add argument into _argFids, _argValues
        private void AddArg(string fid, object value)
        {
            _sqlArgs.Add(fid);
            _sqlArgs.Add(value);
        }

        /// <summary>
        /// get all rows for export excel
        /// </summary>
        /// <param name="crud"></param>
        /// <param name="cond"></param>
        /// <param name="exportSqlFirst">true:先考慮ExcelSql, false: 只考慮Sql</param>
        /// <returns></returns>
        public JArray GetAllRows(ReadDto crud, JObject cond, bool exportSqlFirst)
        {
            //convert sql to model
            var sql = (exportSqlFirst && !string.IsNullOrEmpty(crud.ExportSql))
                ? crud.ExportSql : crud.ReadSql;
            var sqlModel = _Sql.SqlToDto(sql, crud.UseSquare);
            //if (sqlModel0 == null)
            //    return null;

            //prepare sql where, also set _sqlArgs
            //var where = GetWhere(crud.TableAlias, crud.Items, crud.FindCols);
            //var sqlModel = sqlModel0.Value;
            var where = GetWhere(crud, cond);
            if (where != "")
                sqlModel.Where = (sqlModel.Where == "") ? "Where " + where : sqlModel.Where + " And " + where;

            //get rows count if need
            //JArray rows = null;
            using (var db = GetDb())
            {
                //SqlArgModel dbModel = null;

                //set dbModel, and check crud.ColList
                var flag = false;
                if (sqlModel.Select.Contains("DISTINCT")|| sqlModel.Select.Contains("distinct"))
                {
                    flag = true;
                    sqlModel.Select= sqlModel.Select.Replace("DISTINCT", "").Replace("distinct","");
                }

                //get data
                sql = "Select " + (flag ? "distinct " : " ") +
                    "Top 3000 " +
                    sqlModel.Select + " " +
                    sqlModel.From + " " +
                    sqlModel.Where + " " +
                    sqlModel.Group + " " +
                    sqlModel.Order;
                return db.GetJsons(sql, _sqlArgs);
            }
        }

        /// <summary>
        /// TODO: change to return SqlArgModel !!
        /// can directly call(no need to set Crud.Sql !!), return where string
        /// set sql args same time
        /// date field must change format
        /// </summary>
        /// <param name="cond">query condition</param>
        /// <param name="inputSearch">quick search string</param>
        /// <returns></returns>
        public string GetWhere(ReadDto crud, JObject cond, string inputSearch = "")
        {
            #region variables
            var groupLen = (crud.OrGroups == null || crud.OrGroups.Count == 0) ? 0 : crud.OrGroups.Count;
            var orWheres = new string[groupLen == 0 ? 1 : groupLen];
            var where = "";
            var thisWhere = "";
            var and = "";
            var okDates = new List<string>();     //date field be done(date always has start/end)
            var items = crud.Items;
            #endregion

            #region 1.where add condition
            if (items != null && items.Length > 0 && cond != null)
            {
                var table = string.IsNullOrEmpty(crud.TableAs) ? "" : (crud.TableAs + ".");
                foreach (var prop in cond)
                {
                    //skip if empty
                    object value = prop.Value;
                    //var checkvalues = value.ToString().Replace(" ", "").Split(',');
                    if (_Str.IsEmpty(value))
                        continue;

                    #region if query field not existed(and field name is not underline), skip & log
                    var item = items.Where(a => a.Fid == prop.Key).FirstOrDefault();
                    if (item == null)
                    {
                        //1.underline field(reserve field), 2.done date field
                        var key = prop.Key;
                        var len = key.Length;
                        //field name not underline
                        if (key.Substring(0, 1) == "_")
                            continue;

                        //field name tail is 2, for date field
                        if (key.Substring(len - 1, 1) == "2")
                        {
                            //skip if date field is done
                            var key2 = key.Substring(0, len - 1);
                            if (okDates.Contains(key2))
                                continue;

                            //find item
                            item = items.Where(a => a.Fid == key2).FirstOrDefault();
                            if (item == null)
                            {
                                _Log.Error("CrudRead.cs GetWhere() failed: no Fid = " + key2);
                                continue;
                            }

                            //continue
                        }
                        else
                        {
                            //else case, skip & log error
                            _Log.Error("CrudRead.cs GetWhere() failed: no Fid = " + prop.Key);
                            continue;
                        }
                    }
                    #endregion

                    /*
                    //check empty ??
                    var checkFlag = 0;
                    var checkValue = value.ToString().Replace(" ", "").Split(',');
                    for (var i = 0; i < checkValue.Length; i++)
                    {
                        if (checkValue[i] == "")
                        {
                            checkFlag = 1;
                            break;
                        }
                        else
                            checkFlag = 0;
                    }
                    if (checkFlag == 1)
                        continue;
                    */

                    #region set where & add argument
                    //var item = item0.Value;
                    var col = _Str.IsEmpty(item.Col) ? (table + item.Fid) : item.Col;
                    if (item.Op == ItemOpEstr.In)
                    {
                        //"in" has different args type !!
                        //change carrier sign to comma for TextArea field
                        var value2 = value.ToString().Replace(" ", "").Replace("\n", ",");
                        var values = value2.Split(',');
                        var names = new List<string>();
                        for (var i = 0; i < values.Length; i++)
                        {
                            if (_Str.IsEmpty(values[i]))
                                continue;

                            var fid = item.Fid + i;
                            AddArg(fid, values[i]);
                            names.Add("@" + fid);
                        }
                        if (names.Count == 0)
                            continue;
                        thisWhere = col + " in (" + string.Join(",", names) + ")";
                    }
                    else if (item.Op == ItemOpEstr.Like2)
                    {
                        AddArg(item.Fid, "%" + value.ToString() + "%");
                        thisWhere = col + " Like @" + item.Fid;
                    }
                    else if (item.Op == ItemOpEstr.LikeList)
                    {
                        var itemWhere = "";
                        var or = "";
                        var values = value.ToString().Replace(" ", "").Split(',');
                        for (var i = 0; i < values.Length; i++)
                        {
                            if (_Str.IsEmpty(values[i]))
                                continue;

                            var fid = item.Fid + i;
                            itemWhere += or + col + " Like @" + fid;
                            or = " Or ";
                            AddArg(fid, values[i] + "%");
                        }
                        if (itemWhere == "")
                            continue;
                        thisWhere = "(" + itemWhere + ")";
                    }
                    else if (item.Op == ItemOpEstr.LikeCols || item.Op == ItemOpEstr.Like2Cols)
                    {
                        var pre = (item.Op == ItemOpEstr.Like2Cols) ? "%" : "";
                        var itemWhere = "";
                        var or = "";
                        var cols = col.Replace(" ", "").Split(',');
                        foreach (var col2 in cols)
                        {
                            if (_Str.IsEmpty(col2))
                                continue;

                            itemWhere += or + col2 + " Like @" + item.Fid;
                            or = " Or ";
                        }
                        if (itemWhere == "")
                            continue;

                        AddArg(item.Fid, pre + value + "%");
                        thisWhere = "(" + itemWhere + ")";
                    }
                    else if (item.Op == ItemOpEstr.Is)
                    {
                        if (value.ToString() != "1")
                            thisWhere = col + " is Null";
                        else if (value.ToString() != "0")
                            thisWhere = col + " is not Null";
                        else
                            thisWhere = col + " is " + value;
                    }
                    else if (item.Op == ItemOpEstr.IsNull)
                    {
                        if (value.ToString() != "1")
                            continue;
                        thisWhere = col + " is Null";
                    }
                    else if (item.Op == ItemOpEstr.NotNull)
                    {
                        if (value.ToString() != "1")
                            continue;
                        thisWhere = col + " is not Null";
                    }
                    else if (item.Op == ItemOpEstr.UserDefined)
                    {
                        //add () sign
                        thisWhere = "(" + col + " " + value.ToString() + ")";
                    }
                    else if (item.Op == ItemOpEstr.InRange)
                    {
                        var fid2 = item.Fid + "2";
                        var hasNum1 = !_Str.IsEmpty(cond[item.Fid]);
                        var hasNum2 = !_Str.IsEmpty(cond[fid2]);

                        okDates.Add(item.Fid);

                        if (hasNum1 && hasNum2)
                        {
                            //thisWhere = string.Format("({0} >= @{1} And {0} <= @{2})", col, item.Fid, fid2);
                            thisWhere = $"({col} >= @{item.Fid} And {col} <= @{fid2})";

                            //add arg
                            AddArg(item.Fid, value.ToString());
                            AddArg(fid2, cond[fid2].ToString());
                        }
                        else if (hasNum1)
                        {
                            //thisWhere = string.Format("({0} >= @{1})", col, item.Fid);
                            thisWhere = $"({col} >= @{item.Fid})";

                            //add arg
                            AddArg(item.Fid, value.ToString());
                        }
                        else if (hasNum2)
                        {
                            //thisWhere = string.Format("({0} <= @{1})", col, fid2);
                            thisWhere = $"({col} <= @{fid2})";

                            //add arg
                            AddArg(fid2, cond[fid2].ToString());
                        }
                    }
                    /*
                    //date field(consider performance)
                    else if (item.Type == EnumItemType.Date)
                    {
                        var date1 = _Locale.FrontToBack(value.ToString());
                        var date2 = date1.AddDays(1);
                        where += and + GetWhereByDate(item, col, date1, date2);
                    }
                    */
                    //2 date fields, fid tail must be 2 !! ex:StartDate, StartDate2
                    else if (item.Type == QitemTypeEnum.Date)
                    {
                        //if date2 is done, just skip
                        var len = item.Fid.Length;
                        //var fid = item.Fid.Substring(0, len - 1);
                        //if (item.Fid.Substring(len - 1, 1) == "2" && okDates.Contains(fid))
                        //    continue;

                        //log date2 is done
                        okDates.Add(item.Fid);

                        //get where
                        var fid2 = item.Fid + "2";
                        var hasDate1 = !_Str.IsEmpty(cond[item.Fid]);
                        var hasDate2 = !_Str.IsEmpty(cond[fid2]);
                        if (hasDate1 && hasDate2)  //case of has 2nd field, then query start/end
                        {
                            //thisWhere = "(" + col + " Between @" + item.Fid + " And @" + fid2 + ")";
                            //thisWhere = string.Format("({0} is Null Or {0} Between @{1} And @{2})", col, item.Fid, fid2);
                            thisWhere = $"({col} is Null Or {col} Between @{item.Fid} And @{fid2})";

                            //add arg
                            AddArg(item.Fid, _Date.CsToDt(value.ToString() + " 00:00:00"));
                            AddArg(fid2, _Date.CsToDt(cond[fid2].ToString() + " 23:59:59"));
                        }
                        else if (hasDate1)  //has start date, then query this date after
                        {
                            //get where
                            //thisWhere = "(" + col + " >= @" + item.Fid + " And " + col + " < @" + fid2 + ")";
                            //thisWhere = "(" + col + " >= @" + item.Fid + ")";
                            //thisWhere = string.Format("({0} is Null Or {0} >= @{1})", col, item.Fid);
                            thisWhere = $"({col} is Null Or {col} >= @{item.Fid})";

                            //Datetime only read date part, type is string
                            var date1 = _Date.CsToDt(value.ToString());
                            AddArg(item.Fid, _Str.GetLeft(date1.ToString(), " "));
                            //AddArg(fid2, _Str.RemovePart(date1.AddDays(1).ToString(), " "));
                        }
                        else if (hasDate2)  //has end date, then query this date before
                        {
                            //get where
                            //thisWhere = "(" + col + " >= @" + item.Fid + " And " + col + " < @" + fid2 + ")";
                            //thisWhere = "(" + col + " <= @" + fid2 + ")";
                            //thisWhere = string.Format("({0} is Null Or {0} <= @{1})", col, fid2);
                            thisWhere = $"({col} is Null Or {col} <= @{fid2})";

                            //Datetime field only get date part, type is string
                            var date1 = _Date.CsToDt(value.ToString());
                            AddArg(fid2, _Str.GetLeft(date1.ToString(), " ") + " 23:59:59");
                        }
                    }
                    //2 date fields, if input one date then must in range, if input 2 dates, then must be intersection with start/end
                    //ex: StartDate, EndDate, no consider item.Op !!
                    else if (item.Type == QitemTypeEnum.Date2)
                    {
                        //if Date2 field not set "Other", log error & skip
                        //var item2 = new ReadItemCrud();
                        var fid2 = item.Fid + "2";
                        var col2 = item.Other;
                        if (String.IsNullOrEmpty(col2))
                        {
                            _Log.Error("CrudRead.cs GetWhere() failed: no Other field for Date2 (" + item.Fid + ")");
                            continue;
                        }
                        /*
                        else 
                        {
                            //fid2 must existed須存在
                            item2 = (items.FirstOrDefault(a => a.Fid == fid2));
                            if (item2 == null)
                            {
                                _Log.Error("CrudRead.cs GetWhere() failed: Other field not existed (" + fid2 + ")");
                                continue;
                            }
                        }
                        */

                        //log date2 is done
                        okDates.Add(item.Fid);

                        //get where
                        //var col2 = _Str.IsEmpty(item2.Col) ? (table + item2.Fid) : item2.Col;
                        var hasDate1 = !_Str.IsEmpty(cond[item.Fid]);
                        var hasDate2 = !_Str.IsEmpty(cond[fid2]);
                        if (hasDate1 && hasDate2)  //case of 2nd field, then query start/end
                        {
                            //thisWhere = "(" + col + " <= @" + fid2 + " And " + col2 + " >= @" + item.Fid + ")";
                            //thisWhere = string.Format("(({0} is Null Or {0} <= @{1}) And ({2} is Null Or {2} >= @{3}))", col, fid2, col2, item.Fid);
                            thisWhere = $"(({col} is Null Or {col} <= @{fid2}) And ({col2} is Null Or {col2} >= @{item.Fid}))";

                            //add arg
                            AddArg(fid2, _Str.GetLeft(_Date.CsToDt(cond[fid2].ToString()).ToString(), " ") + " 23:59:59");
                            AddArg(item.Fid, _Str.GetLeft(_Date.CsToDt(value.ToString()).ToString(), " "));
                        }
                        else if (hasDate1)  //only start date, then query bigger than this date
                        {
                            //get where
                            //"(" + col2 + " >= @" + item.Fid + ")";
                            //thisWhere = string.Format("({0} is Null or {0} >= @{1})", col2, item.Fid);
                            thisWhere = $"({col2} is Null or {col2} >= @{item.Fid})";

                            //get date part of Datetime
                            var date1 = _Date.CsToDt(value.ToString());
                            AddArg(item.Fid, _Str.GetLeft(date1.ToString(), " "));
                            //AddArg(fid2, _Str.RemovePart(date1.AddDays(1).ToString(), " "));
                        }
                        else if (hasDate2)  //only end date, then query small than this date
                        {
                            //get where
                            //thisWhere = "(" + col + " <= @" + fid2 + ")";
                            //thisWhere = string.Format("({0} is Null Or {0} <= @{1})", col, fid2);
                            thisWhere = $"({col} is Null Or {col} <= @{fid2})";

                            //get date part of Datetime
                            var date1 = _Date.CsToDt(value.ToString());
                            AddArg(fid2, _Str.GetLeft(date1.ToString(), " ") + " 23:59:59");
                            //AddArg(fid2, _Str.RemovePart(date1.AddDays(1).ToString(), " "));
                        }
                    }
                    else if (item.Op == ItemOpEstr.Equal)
                    {
                        thisWhere = col + " = @" + item.Fid;
                        AddArg(item.Fid, value);
                    }
                    else if (item.Op == ItemOpEstr.Like)
                    {
                        thisWhere = col + " like @" + item.Fid;
                        AddArg(item.Fid, value + "%");
                    }
                    else if (item.Op == ItemOpEstr.NotLike)
                    {
                        thisWhere = col + " not like @" + item.Fid;
                        AddArg(item.Fid, value + "%");
                    }
                    else
                    {
                        //直接讓它產生查詢錯誤!!
                        thisWhere = col + " " + item.Op + " @" + item.Fid;
                        AddArg(item.Fid, value);
                    }
                    #endregion

                    #region consider OrGroups
                    var findGroup = -1;
                    if (groupLen > 0)
                    {
                        for (var i = 0; i < groupLen; i++)
                        {
                            var group = crud.OrGroups[i];
                            if (group.Contains(item.Fid))
                            {
                                findGroup = i;
                                break;
                            }
                        }
                    }
                    if (findGroup >= 0)
                        orWheres[findGroup] += thisWhere + " Or ";
                    else
                    {
                        where += and + thisWhere;
                        and = " And ";
                    }
                    #endregion

                    //set others variables
                    //wheres.Add(col);
                } //foreach

                //add orWheres[] into where
                if (groupLen > 0)
                {
                    foreach (var orWhere in orWheres)
                    {
                        if (!_Str.IsEmpty(orWhere))
                        {
                            where += and + "(" + orWhere.Substring(0, orWhere.Length - 4) + ")";
                            and = " And ";
                        }
                    }
                }
            }
            #endregion

            #region 2.where add quick search
            //var search = (_dtIn.search == null) ? "" : _dtIn.search.value;
            var search = inputSearch;
            if (!_Str.IsEmpty(search))
            {
                //by finding string
                var itemWhere = "";
                and = "";
                foreach (var col2 in crud.FindCols)
                {
                    //_stCache.readFids.Add(_list.findCols[i]);
                    itemWhere += and + col2 + " Like @" + _FindId;
                    and = " Or ";

                    //add wheres[]
                    //wheres.Add(col2);
                }

                //add where
                itemWhere = "(" + itemWhere + ")";
                if (where == "")
                    where = itemWhere;
                else
                    where += " And " + itemWhere;

                //add argument
                AddArg(_FindId, "%" + search + "%");
            }
            #endregion

            return where;
        }

        #region remark code    
        /*
        public MemoryStream ExportExcel(ReadCrud crud, JObject findJson, string sheetName, List<string> headers = null, List<string> cols = null)
        {
            var rows = GetAllRows(crud, findJson);
            return _Excel.JsonsToExcel(sheetName, rows, headers, cols);
        }
        */

        /// <summary>
        /// 傳回dataTables資料, 含多筆Json資料列
        /// </summary>
        /// <param name="crud"></param>
        /// <param name="dtIn"></param>
        /// <param name="findJson"></param>
        /// <returns></returns>
        /*
        public object GetModelRows<T>(CrudJsonModel crud, DataTableIn dtIn, object findJson) where T : class
        {
            //設定傳入參數
            _dtIn = dtIn;
            _findJson = findJson;

            //convert sql to model
            var sqlModel = _Sql.SqlToModel(crud.Sql);

            //get sql where
            var where = GetWhere(crud.Items, crud.FindCols);
            where = (where == "")
                ? sqlModel.Where
                : (sqlModel.Where == "") ? "Where " + where : sqlModel.Where + " And " + where;

            //計算筆數 if need (findCount)
            string sql;
            //var table = typeof(T).Name;
            //var findCount = dtIn.findCount;
            var rowCount = 0;
            if (dtIn.recordsFiltered < 0)
            {
                if (!InitDb(crud.Db))
                    return null;

                sql = "SELECT COUNT(*) AS _count " + sqlModel.From + " WITH (NOLOCK) " + where;
                var row = _db.GetJsonRow(sql);
                rowCount = (row == null) ? 0 : Convert.ToInt32(row["_count"]);
            }

            //計算起迄資料序號
            var row1 = dtIn.start;
            var row2 = dtIn.start + dtIn.length - 1;
            AddArg("row1", row1);
            AddArg("row2", row2);

            //get order, 考慮傳入條件
            //var order = this.Crud.Order;

            //sql for 查詢一頁資料, 0:select, 1:where, 2:order
            //如果使用cache, 則讀取所有欄位
            //var selectFrom = sqlModel.Select + " " + sqlModel.From;
            //var select = _useCache ? this.Crud.SelectFrom : "*";
            //where = (sqlModel.Where == "") ? "Where " + where : sqlModel.Where + " And " + where;
            sql = String.Format(_Fun2.CrudSql, sqlModel.Select, sqlModel.From, where, sqlModel.Order);
            //var rows = db.GetModelRows<TOut>(sql, _argFids, _argValues);
            if (!InitDb(crud.Db))
                return null;

            //var rows = _db.GetModelRows<T>(new DbReadModel()
            var rows = _db.GetObjectRows(new DbReadModel()
            {
                Sql = sql,
                ArgFids = _argFids,
                ArgValues = _argValues
            });

            //close db
            _db.Dispose();

            //return new DataTableOut<dynamic>()
            return new DataTableOut()
            {
                draw = dtIn.draw,
                data = rows,
                recordsTotal = 0,
                recordsFiltered = rowCount,
            };

            //labError:
            //    return null;
            //return null;
        }
        */

        /*
        //處理日期欄位的 where 條件
        private string GetWhereByDate(CrudItemModel item, string col, DateTime date1, DateTime date2)
        {
            var where = "(" + col + " Between @" + item.Fid + "1 And @" + item.Fid + "2)";
            //var date1 = _Locale.FrontToBack(value.ToString());
            //var date2 = date1.AddDays(1);
            //Datetime欄位只取前面的日期部分, 格式為字串
            AddArg(item.Fid + "1", _Str.RemovePart(date1.ToString(), " "));
            AddArg(item.Fid + "2", _Str.RemovePart(date2.ToString(), " "));
            return where;
        }
        */
        #endregion

    }//class
}

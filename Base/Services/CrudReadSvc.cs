using Base.Enums;
using Base.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// for Crud Read Service
    /// </summary>
    public class CrudReadSvc
    {
        //search field name for sql args
        private const string _FindFid = "_find";

        //db str in config file
        private readonly string _dbStr = "";
        private readonly Db _db = null!;

        //jQuery dataTables input arg
        //private DtDto _dtDto;

        //sql args, (id, value), be set in GetWhereAsync()
        private List<object> _sqlArgs = new();

        //constructor
        public CrudReadSvc(string dbStr = "")
        {
            _dbStr = dbStr;
        }

        //constructor, db can not be null
        public CrudReadSvc(Db db)
        {
            _db = db;
        }

        private Db GetDb()
        {
            return _db ?? new Db(_dbStr);
        }

        /// <summary>
        /// get page rows for dataTables
        /// </summary>
        /// <param name="readDto"></param>
        /// <param name="dtDto"></param>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        public async Task<JObject?> GetPageA(ReadDto readDto, DtDto dtDto, string ctrl = "")
        {
            var easyDto = _Model.Copy<DtDto, EasyDtDto>(dtDto);
            if (string.IsNullOrEmpty(dtDto.sort) && dtDto.order != null && dtDto.order.Count > 0)
            {
                /*old code 參考
                var orderCol = (dtDto.order == null || dtDto.order.Count == 0)
                    ? -1 : dtDto.order[0].column;
                if (orderCol >= 0)
                    sqlDto.Order = "Order By " +
                        sqlDto.Columns[orderCol].Trim() +
                        (dtDto.order![0].dir == OrderTypeEnum.Asc ? "" : " Desc");
                */

                //A/D + fidNo(base 0) for jquery dataTables
                easyDto.sort = (dtDto.order![0].dir == OrderTypeEnum.Asc ? "A" : "D") +
                    dtDto.order[0].fid;
            }
            return await GetPageA(readDto, easyDto, ctrl);
        }

        /// <summary>
        /// get page rows for normal case
        /// </summary>
        /// <param name="readDto"></param>
        /// <param name="dtDto"></param>
        /// <param name="ctrl">controller name for authorize</param>
        /// <returns>jquery dataTables object</returns>
        public async Task<JObject?> GetPageA(ReadDto readDto, EasyDtDto dtDto, string ctrl = "")
        {
            #region 1.check input
            dtDto.length = _Page.GetPageRows(dtDto.length);

            //set instance variables from input args
            //_dtDto = dtDto;
            //if (findJson == null)
            //    findJson = _Str.IsEmpty(dtDto.findJson) ? null : _Str.ToJson(dtDto.findJson);
            #endregion

            #region 2.get sql
            if (readDto.ReadSql == "") return null;

            var sqlDto = _Sql.SqlToDto(readDto.ReadSql, readDto.UseSquare);
            //prepare sql where & set sql args by user input condition
            //var search = (dtDto.search == null) ? "" : dtDto.search.value;
            var where = GetWhere(ctrl, readDto, _Str.ToJson(dtDto.findJson), CrudEnum.Read, dtDto.search2);
            if (where == "-1") return _Json.GetError();
            if (where == "-2") return _Json.GetBrError(_Fun.FidTimeOut);

            if (where != "")
                sqlDto.Where = (sqlDto.Where == "") 
                    ? "Where " + where : sqlDto.Where + " And " + where;
            #endregion

            #region 3.get rows count if need
            JArray? rows = null;
            var db = GetDb();
            var rowCount = dtDto.recordsFiltered;
            var group = (sqlDto.Group == "") ? "" : " " + sqlDto.Group; //remove last space
            string sql;
            if (rowCount < 0)
            {
                sql = "Select Count(*) as _count " +
                    sqlDto.From + " " +
                    sqlDto.Where +
                    group;
                var row = await db.GetRowA(sql, _sqlArgs); //for log carrier
                if (row == null)
                {
                    rowCount = 0;
                    goto lab_exit;
                }

                //case of ok
                rowCount = Convert.ToInt32(row["_count"]);
            }
            #endregion

            #region 4.sql add sorting
            //優先考慮 sort 欄位, 內容為: A/D+fid, ex:Au.Account
            if (_Str.NotEmpty(dtDto.sort))
            {
                //jquery dataTables 傳入排序欄位號碼
                var sortFid = dtDto.sort[1..];
                //if (int.TryParse(sortFid, out var sortFno)) sortFid = sqlDto.Columns[sortFno].Trim();
                sqlDto.Order = "Order By " + sortFid + 
                    (dtDto.sort[..1] == "D" ? " Desc" : "");
            }
            #endregion

            #region 5.get page rows 
            sql = _Sql.DtoToSql(sqlDto, dtDto.start, dtDto.length);
            rows = await db.GetRowsA(sql, _sqlArgs);
            #endregion

        lab_exit:
            //close db
            await db.DisposeAsync();

            //return result
            return JObject.FromObject(new
            {
                //dtDto.draw,
                data = rows,
                recordsFiltered = rowCount,
            });
        }

        /*
        /// <summary>
        /// ??for jquery/app/vue simple pagination
        /// get page rows for dataTables(json)
        /// logic same to GetPageA()
        /// </summary>
        /// <param name="readDto"></param>
        /// <param name="pageIn"></param>
        /// <param name="ctrl">controller name for authorize</param>
        /// <returns>jquery dataTables object</returns>
        public async Task<PageOut<T>?> GetPage2A<T>(ReadDto readDto, PageIn pageIn, string ctrl = "") where T : class
        {
            #region 1.check input
            //adjust page rows if need
            //if (pageIn.length < 1)
            //    pageIn.length = _Fun.PageRows;
            //else if (pageIn.length > 100)
            //    pageIn.length = 100;

            var result = new PageOut<T>();
            #endregion

            #region 2.get sql
            if (readDto.ReadSql == "") return null;

            var sqlDto = _Sql.SqlToDto(readDto.ReadSql, readDto.UseSquare);
            //if (sqlDto == null) return null;
            //prepare sql where & set sql args by user input condition
            //var search = (_dtDto.search == null) ? "" : _dtDto.search.value;
            var where = await GetWhereA(ctrl, readDto, _Str.ToJson(pageIn.findJson), CrudEnum.Read);
            if (where == "-1") return _Page.GetError<T>(result);
            else if (where == "-2") return _Page.GetBrError<T>(result, _Fun.FidTimeOut);

            if (where != "")
                sqlDto.Where = (sqlDto.Where == "")
                    ? "Where " + where : sqlDto.Where + " And " + where;
            #endregion

            #region 3.get rows count if need
            List<T>? rows = null;
            var db = GetDb();
            var filterRows = pageIn.filterRows;
            var group = (sqlDto.Group == "") ? "" : " " + sqlDto.Group; //remove last space
            string sql;
            if (filterRows < 0)
            {
                sql = "Select Count(*) as _count " +
                    sqlDto.From + " " +
                    sqlDto.Where +
                    group;
                var row = await db.GetRowA(sql, _sqlArgs); //for log carrier
                if (row == null)
                {
                    filterRows = 0;
                    goto lab_exit;
                }

                //case of ok
                filterRows = Convert.ToInt32(row["_count"]);
            }
            #endregion

            #region 4.sql add sorting
            //var orderColumn = (dtDto.order == null || dtDto.order.Count == 0)
            //    ? -1 : dtDto.order[0].column;
            //if (orderColumn >= 0)
            //    sqlDto.Order = "Order By " +
            //        sqlDto.Columns[orderColumn].Trim() +
            //        (dtDto.order[0].dir == OrderTypeEnum.Asc ? "" : " Desc");
            #endregion

            #region 5.get page rows 
            sql = _Sql.DtoToSql(sqlDto, (pageIn.page - 1) * pageIn.length, pageIn.length);
            rows = await db.GetModelsA<T>(sql, _sqlArgs);
            rows ??= new List<T>();
        #endregion

            lab_exit:
            //close db
            await db.DisposeAsync();

            //return result
            var json = JObject.FromObject(new
            {
                pageNo = pageIn.page,
                pageRows = pageIn.length,
                filterRows,
            });
            result.PageArg = _Json.ToStr(json);
            result.Rows = rows;
            return result;
        }
        */

        /// <summary>
        /// 讀取資料 for 匯出 excel
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="readDto"></param>
        /// <param name="findJson"></param>
        /// <returns></returns>
        public async Task<JArray?> GetExportRowsA(string ctrl, ReadDto readDto, JObject findJson)
        {
            return await GetRows2A(ctrl, CrudEnum.Export, readDto, findJson, _Fun.MaxExportCount, readDto.ExportSql);
        }

        /// <summary>
        /// 讀取資料(不考慮權限), ex:KeyModal資料
        /// </summary>
        /// <param name="readDto"></param>
        /// <param name="findJson"></param>
        /// <param name="readRows"></param>
        /// <returns></returns>
        public async Task<JArray?> GetRowsA(ReadDto readDto, JObject findJson, int readRows)
        {
            return await GetRows2A("", CrudEnum.Empty, readDto, findJson, readRows);
        }

        /// <summary>
        /// get all rows for export excel
        /// </summary>
        /// <param name="ctrl">controller name for authorize</param>
        /// <param name="crudEnum">crud功能種類 for authorize</param>
        /// <param name="readDto"></param>
        /// <param name="findJson"></param>
        /// <param name="readRows">讀取資料筆數</param>
        /// <param name="readSql">如果有值則會用來查詢資料</param>
        /// <returns></returns>
        private async Task<JArray?> GetRows2A(string ctrl, CrudEnum crudEnum, ReadDto readDto, 
            JObject findJson, int readRows, string readSql = "")
        {
            //convert sql to model
            var sql = _Str.IsEmpty(readSql) ? readDto.ReadSql : readSql;
            if (sql == "") return null;

            var sqlDto = _Sql.SqlToDto(sql, readDto.UseSquare);
            //if (sqlDto == null)
            //    return null;

            //prepare sql where, also set _sqlArgs
            var where = GetWhere(ctrl, readDto, findJson, crudEnum);
            //TODO for -2
            if (where == "-1" || where == "-2") return null;

            if (where != "")
                sqlDto.Where = (sqlDto.Where == "") ? "Where " + where : sqlDto.Where + " And " + where;

            //remove distinct
            await using var db = GetDb();
            if (sqlDto.Select.Contains("DISTINCT") || sqlDto.Select.Contains("distinct"))
                sqlDto.Select = sqlDto.Select.Replace("DISTINCT", "").Replace("distinct", "");

            //get data
            sql = _Sql.DtoToSql(sqlDto, 0, readRows);
            return await db.GetRowsA(sql, _sqlArgs);
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
        /// can directly call(no need to set Crud.Sql !!), return where string
        /// set sql args same time
        /// date field must change format
        /// </summary>
        /// <param name="findJson">query condition</param>
        /// <param name="inputSearch">quick search string</param>
        /// <returns>where string, -1(input error), -2(timeout or none BaseUser for Auth)</returns>
        private string GetWhere(string ctrl, ReadDto readDto, JObject? findJson, 
            CrudEnum crudEnum, string inputSearch = "")
        {
            #region set variables
            var groupLen = (readDto.OrGroups == null || readDto.OrGroups.Count == 0) ? 0 : readDto.OrGroups.Count;
            var orWheres = new string[groupLen == 0 ? 1 : groupLen];
            var okDates = new List<string>();     //date field be done(date always has start/end)
            var items = readDto.Items;
            var itemWhere = "";
            string error;
            #endregion

            #region 1.where add condition, when error return "-1"
            var where = "";
            var and = "";
            if (items != null && items.Length > 0 && findJson != null)
            {
                var table = _Str.IsEmpty(readDto.TableAs) ? "" : (readDto.TableAs + ".");
                foreach (var prop in findJson)
                {
                    //skip if empty
                    object value = prop.Value!;
                    //var checkvalues = value.ToString().Replace(" ", "").Split(',');
                    if (_Object.IsEmpty(value)) continue;

                    #region if query field not existed(and field name is not underline), skip & log
                    var item = items.Where(a => a.Fid == prop.Key).FirstOrDefault();
                    if (item == null)
                    {
                        //1.underline field(reserve field), 2.done date field
                        var key = prop.Key;
                        var len = key.Length;
                        //field name not underline
                        if (key[..1] == "_") continue;

                        //field name tail is 2, for date field
                        if (key.Substring(len - 1, 1) == "2")
                        {
                            //skip if date field is done
                            var key2 = key[..(len - 1)];
                            if (okDates.Contains(key2)) continue;

                            //find item
                            item = items.Where(a => a.Fid == key2).FirstOrDefault();
                            if (item == null)
                            {
                                error = "no Fid = " + key2;
                                goto lab_error;
                            }
                        }
                        else
                        {
                            //else case, skip & log error
                            error = "no Fid = " + prop.Key;
                            goto lab_error;
                        }
                    }
                    #endregion

                    #region set where & add argument
                    //var item = item0.Value;
                    var col = _Str.IsEmpty(item.Col) ? (table + item.Fid) : item.Col;
                    //2 date fields, fid tail must be 2 !! ex:StartDate, StartDate2
                    if (item.Type == QitemTypeEnum.Date)
                    {
                        //if date2 is done, just skip
                        //var len = item.Fid.Length;
                        //var fid = item.Fid.Substring(0, len - 1);
                        //if (item.Fid.Substring(len - 1, 1) == "2" && okDates.Contains(fid))
                        //    continue;

                        //log date2 is done
                        okDates.Add(item.Fid);

                        //get where
                        var fid2 = item.Fid + "2";
                        var hasDate1 = _Object.NotEmpty(findJson[item.Fid]);
                        var hasDate2 = _Object.NotEmpty(findJson[fid2]);
                        if (hasDate1 && hasDate2)  //case of has 2nd field, then query start/end
                        {
                            itemWhere = $"({col} is Null Or {col} Between @{item.Fid} And @{fid2})";
                            AddArg(item.Fid, _Date.CsToDt(value.ToString()!)!);
                            AddArg(fid2, _Date.CsToDt(findJson[fid2]!.ToString() + " 23:59:59")!);
                        }
                        else if (hasDate1)  //has start date, then query this date after
                        {
                            itemWhere = $"({col} is Null Or {col} >= @{item.Fid})";

                            //Datetime only read date part, type is string
                            var date1 = _Date.CsToDt(value.ToString()!);
                            AddArg(item.Fid, _Str.GetLeft(date1.ToString()!, " "));
                        }
                        else if (hasDate2)  //has end date, then query this date before
                        {
                            itemWhere = $"({col} is Null Or {col} <= @{fid2})";

                            //Datetime field only get date part, type is string
                            var date1 = _Date.CsToDt(value.ToString()!);
                            AddArg(fid2, _Str.GetLeft(date1.ToString()!, " ") + " 23:59:59");
                        }
                    }
                    //2 date fields, if input one date then must in range, if input 2 dates, then must be intersection with start/end
                    //ex: StartDate, EndDate, no consider item.Op !!
                    else if (item.Type == QitemTypeEnum.Date2)
                    {
                        //if Date2 field not set "Other", log error & skip
                        var fid2 = item.Fid + "2";
                        var col2 = item.Other;
                        if (_Str.IsEmpty(col2))
                        {
                            error = "no Other field for Date2 (" + item.Fid + ")";
                            goto lab_error;
                        }
                        /*
                        else 
                        {
                            //fid2 must existed
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
                        var hasDate1 = _Object.NotEmpty(findJson[item.Fid]);
                        var hasDate2 = _Object.NotEmpty(findJson[fid2]);
                        if (hasDate1 && hasDate2)  //case of 2nd field, then query start/end
                        {
                            itemWhere = $"(({col} is Null Or {col} <= @{fid2}) And ({col2} is Null Or {col2} >= @{item.Fid}))";
                            AddArg(fid2, _Str.GetLeft(_Date.CsToDt(findJson[fid2]!.ToString()).ToString()!, " ") + " 23:59:59");
                            AddArg(item.Fid, _Str.GetLeft(_Date.CsToDt(value.ToString()!).ToString()!, " "));
                        }
                        else if (hasDate1)  //only start date, then query bigger than this date
                        {
                            itemWhere = $"({col2} is Null or {col2} >= @{item.Fid})";

                            //get date part of Datetime
                            var date1 = _Date.CsToDt(value.ToString()!);
                            AddArg(item.Fid, _Str.GetLeft(date1.ToString()!, " "));
                        }
                        else if (hasDate2)  //only end date, then query small than this date
                        {
                            itemWhere = $"({col} is Null Or {col} <= @{fid2})";

                            //get date part of Datetime
                            var date1 = _Date.CsToDt(value.ToString()!);
                            AddArg(fid2, _Str.GetLeft(date1.ToString()!, " ") + " 23:59:59");
                        }
                    }
                    else if (item.Op == ItemOpEstr.Equal)
                    {
                        itemWhere = col + "=@" + item.Fid;
                        AddArg(item.Fid, value);
                    }
                    else if (item.Op == ItemOpEstr.Like)
                    {
                        itemWhere = col + " like @" + item.Fid;
                        AddArg(item.Fid, value + "%");
                    }
                    else if (item.Op == ItemOpEstr.NotLike)
                    {
                        itemWhere = col + " not like @" + item.Fid;
                        AddArg(item.Fid, value + "%");
                    }
                    else if (item.Op == ItemOpEstr.In)
                    {
                        //"in" has different args type !!
                        //change carrier sign to comma for TextArea field
                        var value2 = value.ToString()!.Replace(" ", "").Replace("\n", ",");
                        var values = value2.Split(',');
                        var names = new List<string>();
                        for (var i = 0; i < values.Length; i++)
                        {
                            if (_Str.IsEmpty(values[i])) continue;

                            var fid = item.Fid + i;
                            AddArg(fid, values[i]);
                            names.Add("@" + fid);
                        }
                        if (names.Count == 0) continue;
                        itemWhere = col + " in (" + string.Join(",", names) + ")";
                    }
                    else if (item.Op == ItemOpEstr.Like2)
                    {
                        AddArg(item.Fid, "%" + value.ToString() + "%");
                        itemWhere = col + " Like @" + item.Fid;
                    }
                    else if (item.Op == ItemOpEstr.LikeList)
                    {
                        var where2 = "";
                        var or = "";
                        var values = value.ToString()!.Replace(" ", "").Split(',');
                        for (var i = 0; i < values.Length; i++)
                        {
                            if (_Str.IsEmpty(values[i])) continue;

                            var fid = item.Fid + i;
                            where2 += or + col + " Like @" + fid;
                            or = " Or ";
                            AddArg(fid, values[i] + "%");
                        }
                        if (where2 == "") continue;
                        itemWhere = "(" + where2 + ")";
                    }
                    else if (item.Op == ItemOpEstr.LikeCols || item.Op == ItemOpEstr.Like2Cols)
                    {
                        var pre = (item.Op == ItemOpEstr.Like2Cols) ? "%" : "";
                        var where2 = "";
                        var or = "";
                        var cols = col.Replace(" ", "").Split(',');
                        foreach (var col2 in cols)
                        {
                            if (_Str.IsEmpty(col2)) continue;

                            where2 += or + col2 + " Like @" + item.Fid;
                            or = " Or ";
                        }
                        if (where2 == "") continue;

                        AddArg(item.Fid, pre + value + "%");
                        itemWhere = "(" + where2 + ")";
                    }
                    else if (item.Op == ItemOpEstr.Is)
                    {
                        if (value.ToString() != "1") itemWhere = col + " is Null";
                        else if (value.ToString() != "0") itemWhere = col + " is not Null";
                        else itemWhere = col + " is " + value;
                    }
                    else if (item.Op == ItemOpEstr.IsNull)
                    {
                        if (value.ToString() != "1") continue;
                        itemWhere = col + " is Null";
                    }
                    else if (item.Op == ItemOpEstr.NotNull)
                    {
                        if (value.ToString() != "1") continue;
                        itemWhere = col + " is not Null";
                    }
                    else if (item.Op == ItemOpEstr.UserDefined)
                    {
                        itemWhere = "(" + col + " " + value.ToString() + ")";
                    }
                    else if (item.Op == ItemOpEstr.InRange)
                    {
                        var fid2 = item.Fid + "2";
                        var hasNum1 = _Object.NotEmpty(findJson[item.Fid]);
                        var hasNum2 = _Object.NotEmpty(findJson[fid2]);

                        okDates.Add(item.Fid);

                        if (hasNum1 && hasNum2)
                        {
                            itemWhere = $"({col} >= @{item.Fid} And {col} <= @{fid2})";
                            AddArg(item.Fid, value.ToString()!);
                            AddArg(fid2, findJson[fid2]!.ToString());
                        }
                        else if (hasNum1)
                        {
                            itemWhere = $"({col} >= @{item.Fid})";
                            AddArg(item.Fid, value.ToString()!);
                        }
                        else if (hasNum2)
                        {
                            itemWhere = $"({col} <= @{fid2})";
                            AddArg(fid2, findJson[fid2]!.ToString());
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
                    else
                    {
                        //let it sql wrong!!
                        itemWhere = col + " " + item.Op + " @" + item.Fid;
                        AddArg(item.Fid, value);
                    }
                    #endregion

                    #region consider OrGroups
                    var findGroup = -1;
                    if (groupLen > 0)
                    {
                        for (var i = 0; i < groupLen; i++)
                        {
                            var group = readDto.OrGroups![i];
                            if (group.Contains(item.Fid))
                            {
                                findGroup = i;
                                break;
                            }
                        }
                    }
                    if (findGroup >= 0)
                        orWheres[findGroup] += itemWhere + " Or ";
                    else
                    {
                        where += and + itemWhere;
                        and = " And ";
                    }
                    #endregion

                } //foreach findJson

                //add orWheres[] into where
                if (groupLen > 0)
                {
                    foreach (var orWhere in orWheres)
                    {
                        if (!_Str.IsEmpty(orWhere))
                        {
                            where += and + "(" + orWhere[0..^4] + ")";
                            and = " And ";
                        }
                    }
                }
            }//if
            #endregion

            #region 2.where add for AuthType=Row if need, 同時檢查登入狀態, when error return "-2"
            if (_Fun.IsAuthRowAndLogin() && _Str.NotEmpty(ctrl))
            {
                //權限等級為資料, 但是沒有登入userId
                var baseUser = _Fun.GetBaseUser();
                if (baseUser.UserId == "") return "-2";

                var range = _Auth.GetAuthRange(baseUser.ProgAuthStrs, ctrl, crudEnum);
                if (range == AuthRangeEnum.User)
                {
                    //by user
                    where += and + string.Format(readDto.WhereUserFid, baseUser.UserId);
                    and = " And ";
                }
                else if (range == AuthRangeEnum.Dept)
                {
                    //by depart
                    where += and + string.Format(readDto.WhereDeptFid, baseUser.DeptId);
                    and = " And ";
                }
            }
            #endregion

            #region 3.where add quick search
            //var search = (_dtIn.search == null) ? "" : _dtIn.search.value;
            var search = inputSearch;
            if (!_Str.IsEmpty(search))
            {
                //by finding string
                itemWhere = "";
                and = "";
                foreach (var col2 in readDto.FindCols!)
                {
                    //_stCache.readFids.Add(_list.findCols[i]);
                    itemWhere += and + col2 + " Like @" + _FindFid;
                    and = " Or ";
                }

                //add where
                itemWhere = "(" + itemWhere + ")";
                if (where == "")
                    where = itemWhere;
                else
                    where += " And " + itemWhere;

                //add argument
                AddArg(_FindFid, "%" + search + "%");
            }
            #endregion

            //case of ok
            return where;

        lab_error:
            _Log.Error("CrudRead.cs GetWhere() failed: " + error);
            return "-1";
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
                var row = _db.GetRow(sql);
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
                //recordsTotal = 0,
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

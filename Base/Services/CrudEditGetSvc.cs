using Base.Enums;
using Base.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// 1.remove cache function
/// 2.add read/write multiple table fun
/// </summary>
namespace Base.Services
{
    /// <summary>
    /// base class of CrudEditS, CrudGetS
    /// </summary>
    public class CrudEditGetSvc
    {
        //constant
        //front end input json fields:
        //protected const string Rows = "_rows";        //multiple rows
        //protected const string Childs = "_childs";    //child json list

        //master edit
        protected EditDto _editDto = null!;
        protected string _ctrl = "";       //controll name

        //db str in config file
        protected string _dbStr = "";

        //sql args pair(fid,value), 日期欄位為空時寫入null, 否則會變1900/1/1 !!
        protected List<object?> _sqlArgs = new();

        //constructor
        public CrudEditGetSvc(string ctrl, EditDto editDto, string dbStr = "")
        {
            _ctrl = ctrl;
            _editDto = editDto;
            _dbStr = dbStr;
        }

        protected Db GetDb()
        {
            return new Db(_dbStr);
        }

        protected async Task<JObject?> GetJsonByFunA(CrudEnum fun, string key)
        {
            return await GetJsonA(fun, key);
        }

        //add argument into _argFids, _argValues
        protected void AddArg(string fid, object? value)
        {
            _sqlArgs.Add(fid);
            _sqlArgs.Add(value);
        }

        //clear argument
        protected void ResetArg()
        {
            _sqlArgs = new();
        }

        //get where by pkey for query 1st table & updata tables, set sql args at the same time
        //for getRow & update
        protected string GetWhereAndArg(EditDto edit, string key)
        {
            //kid add "_" for avoid conflict when update
            var kid = "_" + edit.PkeyFid;  
            AddArg(kid, key);
            return edit.PkeyFid + "=@" + kid;
        }

        //get select sql 
        protected string GetSql(EditDto edit, string key)
        {
            ResetArg();
            var where = GetWhereAndArg(edit, key);
            return GetSqlByWhere(edit, where);
        }

        /*
        protected string GetSqlByField(EditDto edit, string key)
        {
            return string.Format(edit.ReadSql, key);
        }
        */

        protected string GetSqlByWhere(EditDto edit, string where)
        {
            //add columns list
            var list = "";
            foreach (var item in edit.Items)
                list += (item.Col == "" ? item.Fid : (item.Col + " as " + item.Fid)) + ",";

            list = list[0..^1];

            //get sql
            var order = _Str.IsEmpty(edit.OrderBy) ? "" : " Order By " + edit.OrderBy;
            return "Select " + list + " From " + edit.Table + " Where " + where + order;
        }

        /// <summary>
        /// has _hideKey for CSRF issue, check this field before update row
        /// </summary>
        /// <param name="key"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public async Task<JObject?> GetDbRowA(EditDto edit, string key, Db? db = null)
        {
            //reset sqlArgs first
            //ResetArg();

            //return row & close db if need
            var newDb = _Db.CheckOpenDb(ref db, _dbStr);
            /*
            var sql = _Str.IsEmpty(edit.ReadSql)
                ? GetSql(edit, key)
                : GetSqlByField(edit, key);
            var row = await db!.GetRowA(sql, _sqlArgs!);
            */
            var row = _Str.IsEmpty(edit.ReadSql)
                ? await db!.GetRowA(GetSql(edit, key), _sqlArgs!)
                : await db!.GetRowA(edit.ReadSql, new() { "Id", key });
            await _Db.CheckCloseDbA(db, newDb);
            return row;
        }

        /// <summary>
        /// get rows for multi tables (1 to many)
        /// include: collumns、_childs
        /// note: 1.master table must relat to child table
        /// </summary>
        /// <param name="key">table primary key value</param>
        /// <returns></returns>
        protected async Task<JObject?> GetJsonA(CrudEnum crudEnum, string key)
        {
            if (!_Str.CheckKey(key))
            {
                //await _Log.ErrorAsync("CrudEdit.cs GetJson() failed, key wrong: " + key);
                return null;
            }

            var db = GetDb();
            var result = await GetDbRowA(_editDto, key, db);    //return data
            if (result == null) goto lab_exit;

            //check for AuthType=Row if need
            if (_Fun.IsAuthTypeRow())
            {
                var brError = CheckAuthRow(result, crudEnum);
                if (_Str.NotEmpty(brError))
                {
                    result = _Json.GetBrError(brError);
                    goto lab_exit;
                }
            }

            //get child rows (recursive)
            var editChilds = _editDto.Childs;
            if (editChilds != null && editChilds.Length > 0)
            {
                var childs = new JArray();
                //var keys = new List<string>() { key };
                for (var i = 0; i < editChilds.Length; i++)
                    childs.Add((await GetChildDbJsonLoopA(1, editChilds[i], new() { key }, db))!);
                result[_Fun.FidChilds] = childs;
            }
            
        lab_exit:
            await db.DisposeAsync();
            return result;
        }

        /// <summary>
        /// check AuthType=Row if need
        /// </summary>
        /// <returns>BR error code if any</returns>
        protected string CheckAuthRow(JObject row, CrudEnum crudEnum)
        {
            var range = _XgProg.GetAuthRange(_Fun.GetBaseUser().ProgAuthStrs, _ctrl, crudEnum);
            if (range == AuthRangeEnum.User)
            {
                if (!_Json.IsFidEqual(row, _Fun.FidUser, _Fun.UserId()))
                    return _Fun.NoAuthUser;
            }
            else if (range == AuthRangeEnum.Dept)
            {
                if (!_Json.IsFidEqual(row, _Fun.FidDept, _Fun.DeptId()))
                    return _Fun.NoAuthDept;
            }

            //case else
            return "";            
        }

        /// <summary>
        /// get childs rows(json) from db (recursive)
        /// </summary>
        /// <param name="editLevel">base 0(考慮master table), 傳入值會從1開始(表示第1層child)</param>
        /// <param name="edit"></param>
        /// <param name="keys"></param>
        /// <param name="db"></param>
        /// <returns>JObject with prop: _rows, _childs</returns>
        protected async Task<JObject?> GetChildDbJsonLoopA(int editLevel, EditDto edit, List<string> keys, Db db)
        {
            //get this rows
            //var level1 = (editLevel == 1);
            var emptyReadSql = _Str.IsEmpty(edit.ReadSql);
            JArray? rows;
            if (editLevel == 1)
            {
                //第1層child where 使用 xxx=@Id
                var sql = emptyReadSql
                    ? GetSqlByWhere(edit, edit.FkeyFid + "=@Id")
                    : edit.ReadSql;
                rows = await db.GetRowsA(sql, new() { "Id", keys[0] });
            }
            else
            {
                //第2層child where 使用 xxx in ({0})
                var keyList = _List.ToStr(keys, true);
                var sql = emptyReadSql
                    ? GetSqlByWhere(edit, edit.FkeyFid + $" in ({keyList})")
                    : string.Format(edit.ReadSql, keyList);     //GetSqlByField(edit, keyList);
                rows = await db.GetRowsA(sql);
            }
            /*
            var sql = _Str.IsEmpty(edit.ReadSql)
                ? GetSqlByWhere(edit, edit.FkeyFid + " in (" + keyList + ")")
                : string.Format(edit.ReadSql, keyList);     //GetSqlByField(edit, keyList);
            var rows = await db.GetRowsA(sql);
            */
            if (rows == null) return null;

            //prepare return data
            var data = new JObject() { [_Fun.FidRows] = rows };

            //get childs json list(recursive)
            var editChilds = edit.Childs;
            if (editChilds != null && editChilds.Length > 0)
            {
                keys = _Json.ArrayToListStr(rows, edit.PkeyFid);
                var childs = new JArray();
                for (var i = 0; i < editChilds.Length; i++)
                    childs.Add((await GetChildDbJsonLoopA(editLevel + 1, editChilds[i], keys, db))!);
                data[_Fun.FidChilds] = childs;
            }
            return data;
        }

    }//class
}
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
    /// for Crud Edit Service
    /// </summary>
    public class CrudBase
    {
        //constant
        //front end input json fields:
        //protected const string Rows = "_rows";        //multiple rows
        //protected const string Childs = "_childs";    //child json list

        //master edit
        protected EditDto _editDto;
        protected string _ctrl;       //controll name

        //db str in config file
        protected string _dbStr;

        //sql args pair(fid,value)
        protected List<object> _sqlArgs = new();

        //constructor
        public CrudBase(string ctrl, EditDto editDto, string dbStr = "")
        {
            _ctrl = ctrl;
            _editDto = editDto;
            _dbStr = dbStr;
        }

        protected Db GetDb()
        {
            return new Db(_dbStr);
        }

        protected async Task<JObject> GetJsonByFunAsync(CrudEnum fun, string key)
        {
            return await GetJsonAsync(fun, key);
        }

        //add argument into _argFids, _argValues
        protected void AddArg(string fid, object value)
        {
            _sqlArgs.Add(fid);
            _sqlArgs.Add(value);
        }

        //clear argument
        protected void ResetArg()
        {
            _sqlArgs = new List<object>();
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

        protected string GetSqlByField(EditDto edit, string key)
        {
            return string.Format(edit.ReadSql, key);
            /*
            return _Str.CheckKeyRule(key, edit.ReadSql)
                 ? string.Format(edit.ReadSql, key)
                 : "";
            */
        }

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
        public async Task<JObject> GetDbRowAsync(EditDto edit, string key, Db db = null)
        {
            //reset sqlArgs first
            //ResetArg();

            //connect db if need
            var hasDb = false;
            _Fun.CheckOpenDb(ref db, ref hasDb, _dbStr);

            //return row & close db if need
            var sql = _Str.IsEmpty(edit.ReadSql)
                ? GetSql(edit, key)
                : GetSqlByField(edit, key);
            var row = await db.GetJsonAsync(sql, _sqlArgs);
            await _Fun.CheckCloseDb(db, hasDb);
            return row;
        }

        /// <summary>
        /// get rows for multi tables (1 to many)
        /// include: collumns、_childs
        /// note: 1.master table must relat to child table
        /// </summary>
        /// <param name="key">table primary key value</param>
        /// <returns></returns>
        protected async Task<JObject> GetJsonAsync(CrudEnum crudEnum, string key)
        {
            if (!await _Str.CheckKeyAsync(key))
            {
                //await _Log.ErrorAsync("CrudEdit.cs GetJson() failed, key wrong: " + key);
                return null;
            }

            var db = GetDb();
            var data = await GetDbRowAsync(_editDto, key, db);    //return data
            if (data == null)
                goto lab_exit;

            //check for AuthType=Row if need
            if (_Fun.IsAuthTypeRow())
            {
                var brError = CheckAuthRow(data, crudEnum);
                if (_Str.NotEmpty(brError))
                {
                    data = _Json.GetBrError(brError);
                    goto lab_exit;
                }
            }

            //get child rows (recursive)
            var editChilds = _editDto.Childs;
            if (editChilds != null && editChilds.Length > 0)
            {
                var childs = new JArray();
                var keys = new List<string>() { key };
                for (var i = 0; i < editChilds.Length; i++)
                    childs.Add(await GetChildDbJsonAsync(1, editChilds[i], keys, db));
                data[_Fun.Childs] = childs;
            }
            
        lab_exit:
            await db.DisposeAsync();
            return data;
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
                if (!_Json.IsFidEqual(row, _Fun.UserFid, _Fun.UserId()))
                    return "NoAuthUser";
            }
            else if (range == AuthRangeEnum.Dept)
            {
                if (!_Json.IsFidEqual(row, _Fun.DeptFid, _Fun.DeptId()))
                    return "NoAuthDept";
            }

            //case else
            return "";            
        }

        /// <summary>
        /// get childs rows(json) from db (recursive)
        /// </summary>
        /// <param name="editLevel">base 0, 傳入值會從1開始</param>
        /// <param name="edit"></param>
        /// <param name="keys"></param>
        /// <param name="db"></param>
        /// <returns>JObject with prop: _rows, _childs</returns>
        protected async Task<JObject> GetChildDbJsonAsync(int editLevel, EditDto edit, List<string> keys, Db db)
        {
            //get this rows
            var keyList = _List.ToStr(keys, true);
            var sql = _Str.IsEmpty(edit.ReadSql)
                ? GetSqlByWhere(edit, edit.FkeyFid + " in (" + keyList + ")")
                : GetSqlByField(edit, keyList);
            var rows = await db.GetJsonsAsync(sql);
            if (rows == null)
                return null;

            //prepare return data
            var data = new JObject() { [_Fun.Rows] = rows };

            //get childs json list(recursive)
            var editChilds = edit.Childs;
            if (editChilds != null && editChilds.Length > 0)
            {
                keys = _Json.ArrayToListStr(rows, edit.PkeyFid);
                var childs = new JArray();
                for (var i = 0; i < editChilds.Length; i++)
                    childs.Add(await GetChildDbJsonAsync(editLevel + 1, editChilds[i], keys, db));
                data[_Fun.Childs] = childs;
            }
            return data;
        }

    }//class
}
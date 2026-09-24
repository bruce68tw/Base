using Base.Enums;
using Base.Services;
using Mongo.Models;
using Newtonsoft.Json.Linq;

/// <summary>
/// 1.remove cache function
/// 2.add read/write multiple table fun
/// </summary>
namespace Mongo.Services
{
    /// <summary>
    /// 參考 CrudEditGetSvc
    /// 利用 MgoEditDto 來讀取資料庫資料
    /// base class of CrudEditSvc, CrudGetSvc
    /// </summary>
    public class MgoEditGetSvc
    {

        //master edit
        protected string _ctrl = "";       //controll name

        //db str in config file
        protected string _dbStr = "";

        //只開啟一個db
        protected MgoDb _db = null!;
		protected bool _dbByOut = false;

		//sql args pair(fid,value), 日期欄位為空時寫入null, 否則會變1900/1/1 !!
		//protected List<object?> _sqlArgs = [];

        //constructor
        public MgoEditGetSvc(string ctrl, string dbStr)
        {
            _ctrl = ctrl;
            _dbStr = dbStr;
        }

        /// <summary>
        /// 傳回Db, 外面可呼叫, 建立CRUD Edit/Get 服務時直到存取DB才建立資料庫連線
        /// 如果要自行控制DB關閉, 可傳入 outside=true
        /// </summary>
        /// <param name="outside">外部開啟</param>
        /// <returns></returns>
        public MgoDb GetDb(bool outside = false)
        {
            if (_db == null)
            {
                _dbByOut = outside;
				_db = new MgoDb(_dbStr);
			}
			return _db;
        }

        protected bool CheckOpenDb(ref MgoDb? db)
        {
            return _MgoDb.CheckOpenDb(ref db, _dbStr);
        }

        protected void CheckCloseDb(MgoDb db)
        {
            _MgoDb.CheckCloseDb(db, _dbByOut);
        }

        /*
        //get draft file path by key
        //called CrudGetSvc、CrudEditSvc、others
        public string GetDraftPath(string key)
        {
            key = _Str.EmptyToValue(key, "-1");     //-1表示新增
            return $"{_Fun.DirDraft}{_ctrl}_{_Fun.UserId()}_{key}.json";
        }
        */

        public async Task<JObject?> GetJsonByFunA(CrudEnum fun, string key, MgoEditDto editDto)
        {
            return await GetJsonA(fun, key, editDto);
        }

        /*
        //add argument into _argFids, _argValues
        protected void AddArg(string fid, object? value)
        {
            _sqlArgs.Add(fid);
            _sqlArgs.Add(value);
        }

        //clear argument
        protected void ResetArg()
        {
            _sqlArgs = [];
        }

        //get where by pkey for query 1st table & updata tables, set sql args at the same time
        //for getRow & update
        protected string GetWhereAndArg(MgoEditDto edit, string key)
        {
            //kid add "_" for avoid conflict when update
            var kid = "_" + edit.PkeyFid;  
            AddArg(kid, key);
            return edit.PkeyFid + "=@" + kid;
        }

        //get select sql 
        protected string GetSql(MgoEditDto edit, string key)
        {
            ResetArg();
            var where = GetWhereAndArg(edit, key);
            return GetSqlByWhere(edit, where);
        }

        protected string GetSqlByWhere(MgoEditDto edit, string where)
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
        */

        /// <summary>
        /// has _hideKey for CSRF issue, check this field before update row
        /// </summary>
        /// <param name="key"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public JObject? GetDbRow(MgoEditDto edit, string key, MgoDb? db = null)
        {
            //return row & close db if need
            var newDb = CheckOpenDb(ref db);
            var row = db!.GetRowById(edit.Table, key);
            _MgoDb.CheckCloseDb(db!, newDb);
            return row;
        }

        /// <summary>
        /// 前端傳入與後端傳回的json格式相同，包含rows、childs欄位
        /// get rows for multi tables (1 to many)
        /// include: collumns、_childs
        /// note: 1.master table must relat to child table
        /// </summary>
        /// <param name="key">傳入key值, 有可能不是main table的pkey !!, 例如簽核共用Edit.cs時</param>
        /// <returns></returns>
        protected async Task<JObject?> GetJsonA(CrudEnum fun, string key, MgoEditDto editDto)
        {
            if (!_Str.CheckKey(key)) return null;

            var result = new JObject();
            var db = GetDb();
            var row = GetDbRow(editDto, key);    //return data
            if (row == null) goto lab_exit;

            key = row![editDto.PkeyFid]!.ToString();   //這個才是真正的key !!
            result[_Fun.FidRows] = new JArray(row);

            //check for AuthType=Row if need, "新增"不檢查!!
            if (_Fun.IsAuthRowAndLogin() && fun != CrudEnum.Create)
            {
                var brError = CheckAuthRow(row, fun);
                if (_Str.NotEmpty(brError))
                {
                    result = _Json.GetBrError(brError);
                    goto lab_exit;
                }
            }

            //get child rows (recursive)
            var editChilds = editDto.Childs;
            if (editChilds != null && editChilds.Length > 0)
            {
                var childs = new JArray();
                //var keys = new List<string>() { key };
                for (var i = 0; i < editChilds.Length; i++)
                    childs.Add((await GetChildDbJsonLoopA(1, editChilds[i], [key], db))!);
                result[_Fun.FidChilds] = childs;
            }
            
        lab_exit:
            if (!_dbByOut)
                db.Dispose();
            return result;
        }

        /// <summary>
        /// check AuthType=Row if need
        /// </summary>
        /// <returns>BR error code if any</returns>
        protected string CheckAuthRow(JObject row, CrudEnum fun)
        {
            var range = _Auth.GetAuthRange(_ctrl, fun);
            if (range == AuthRangeEnum.User)
            {
                if (!_Json.IsFidEqual(row, _Fun.FidUser, _Fun.UserId()))
                    return _Fun.FidNoAuthUser;
            }
            else if (range == AuthRangeEnum.Dept)
            {
                if (!_Json.IsFidEqual(row, _Fun.FidDept, _Fun.DeptId()))
                    return _Fun.FidNoAuthDept;
            }

            //case else
            return "";            
        }

        /// <summary>
        /// get child json from db (recursive, 包含rows, childs欄位)
        /// </summary>
        /// <param name="editLevel">base 0(考慮master table), 傳入值會從1開始(表示第1層child)</param>
        /// <param name="edit"></param>
        /// <param name="keys"></param>
        /// <param name="db"></param>
        /// <returns>JObject with prop: _rows, _childs</returns>
        protected async Task<JObject?> GetChildDbJsonLoopA(int editLevel, MgoEditDto edit, List<string> keys, MgoDb db)
        {
            //get this rows
            //var hasReadSql = _Str.NotEmpty(edit.ReadSql);
            JArray? rows = null;
            if (editLevel == 1)
            {
                //第1層child where 使用 xxx=@Id
                var fKeyFid = (edit.FkeyFid == "") ? edit.PkeyFid : edit.FkeyFid;
                //var sql = GetSqlByWhere(edit, fKeyFid + "=@Id");
                var filter = db.PairToFilter(fKeyFid, keys[0]);
                rows = await db.GetRowsByFilterA(edit.Table, filter);
            }
            else
            {
                //todo: 先不考慮第2層以後
                /*
                //第2層child where 使用 xxx in ({0})
                var keyList = _List.ToStr(keys, true);
                var sql = GetSqlByWhere(edit, edit.FkeyFid + $" in ({keyList})");
                rows = await db.GetRowsA(sql);
                */
            }
            if (rows == null) return null;

            //prepare return data
            var result = new JObject() { [_Fun.FidRows] = rows };

            //get childs json list(recursive)
            var editChilds = edit.Childs;
            if (editChilds != null && editChilds.Length > 0)
            {
                keys = _Json.ArrayToListStr(rows, edit.PkeyFid);
                var childs = new JArray();
                for (var i = 0; i < editChilds.Length; i++)
                    childs.Add((await GetChildDbJsonLoopA(editLevel + 1, editChilds[i], keys, db))!);
                result[_Fun.FidChilds] = childs;
            }
            return result;
        }

    }//class
}
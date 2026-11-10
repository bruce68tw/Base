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
    /// CRUD 單筆及主從資料讀取基底，依 EditDto 組成 _rows 與 _childs JSON。
    /// </summary>
    public class CrudEditGetSvc
    {
        //constant
        //front end input json fields:
        //protected const string Rows = "_rows";        //multiple rows
        //protected const string Childs = "_childs";    //child json list

        //master edit
        //protected EditDto _editDto = null!;
        protected string _ctrl = "";       //controll name

        //db str in config file
        protected string _dbStr = "";

        //只開啟一個db
        protected Db _db = null!;
		protected bool _dbByOut = false;

		//sql args pair(fid,value), 日期欄位為空時寫入null, 否則會變1900/1/1 !!
		protected List<object?> _sqlArgs = [];

        /// <summary>
        /// 保存作業名稱與連線字串，尚不建立資料庫連線。
        /// </summary>
        /// <param name="ctrl">對應作業的控制器名稱</param>
        /// <param name="dbStr">資料庫連線字串</param>
        public CrudEditGetSvc(string ctrl, string dbStr = "")
        {
            _ctrl = ctrl;
            //_editDto = editDto;
            _dbStr = dbStr;
        }

        /*
        protected void SetEditDto(EditDto editDto)
        {
            _editDto ??= editDto;
        }
        */

        /// <summary>
        /// 取得或延遲建立 Db；outside 旗標只在建立時設定，釋放行為依實際流程。
        /// </summary>
        /// <param name="outside">新建 Db 時是否標記為外部管理</param>
        public Db GetDb(bool outside = false)
        {
            if (_db == null)
            {
                _dbByOut = outside;
				_db = new Db(_dbStr);
			}
			return _db;
        }

        protected bool CheckOpenDb(ref Db? db)
        {
            return _Db.CheckOpenDb(ref db, _dbStr);
        }

        protected async Task CheckCloseDbA(Db db)
        {
            await _Db.CheckCloseDbA(db, _dbByOut);
        }

        /// <summary>
        /// 組成作業、使用者及資料鍵的草稿 JSON 路徑；空鍵改用 -1。
        /// </summary>
        /// <param name="key">用來定位資料的鍵值</param>
        public string GetDraftPath(string key)
        {
            key = _Str.EmptyToValue(key, "-1");     //-1表示新增
            return $"{_Fun.DirDraft}{_ctrl}_{_Fun.UserId()}_{key}.json";
        }

        /// <summary>
        /// 依指定功能讀取主從資料與資料範圍檢查；結果可為 null、空物件或錯誤 JSON。
        /// </summary>
        /// <param name="fun">要處理的 CRUD 操作類型</param>
        /// <param name="key">用來定位資料的鍵值</param>
        /// <param name="editDto">主從表、主鍵及欄位的 CRUD 維護設定</param>
        public async Task<JObject?> GetJsonByFunA(CrudEnum fun, string key, EditDto editDto)
        {
            return await GetJsonA(fun, key, editDto);
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
            _sqlArgs = [];
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
        /// 讀取主表首列；自訂 ReadSql 使用 @Id，僅釋放此方法自行建立的 Db。
        /// </summary>
        /// <param name="edit">主表的 CRUD 維護設定</param>
        /// <param name="key">用來定位資料的鍵值</param>
        /// <param name="db">要沿用的資料庫物件</param>
        public async Task<JObject?> GetDbRowA(EditDto edit, string key, Db? db = null)
        {
            //reset sqlArgs first
            //ResetArg();

            //return row & close db if need
            var newDb = CheckOpenDb(ref db);
            /*
            var sql = _Str.IsEmpty(edit.ReadSql)
                ? GetSql(edit, key)
                : GetSqlByField(edit, key);
            var row = await db!.GetRowA(sql, _sqlArgs!);
            */
            var row = _Str.IsEmpty(edit.ReadSql)
                ? await db!.GetRowA(GetSql(edit, key), _sqlArgs!)
                : await db!.GetRowA(edit.ReadSql, ["Id", key]);
            await _Db.CheckCloseDbA(db!, newDb);
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
        protected async Task<JObject?> GetJsonA(CrudEnum fun, string key, EditDto editDto)
        {
            if (!_Str.CheckKey(key)) return null;

            var result = new JObject();
            var db = GetDb();
            var row = await GetDbRowA(editDto, key, db);    //return data
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
                await db.DisposeAsync();
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
        protected async Task<JObject?> GetChildDbJsonLoopA(int editLevel, EditDto edit, List<string> keys, Db db)
        {
            //get this rows
            //var level1 = (editLevel == 1);
            var hasReadSql = _Str.NotEmpty(edit.ReadSql);
            JArray? rows;
            if (editLevel == 1)
            {
                //第1層child where 使用 xxx=@Id
                var fKeyFid = (edit.FkeyFid == "") ? edit.PkeyFid : edit.FkeyFid;
                var sql = hasReadSql
                    ? edit.ReadSql
                    : GetSqlByWhere(edit, fKeyFid + "=@Id");
                rows = await db.GetRowsA(sql, ["Id", keys[0]]);
            }
            else
            {
                //第2層child where 使用 xxx in ({0})
                var keyList = _List.ToStr(keys, true);
                var sql = hasReadSql
                    ? string.Format(edit.ReadSql, keyList)
                    : GetSqlByWhere(edit, edit.FkeyFid + $" in ({keyList})");
                rows = await db.GetRowsA(sql);
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

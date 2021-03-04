using Base.Enums;
using Base.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 1.remove cache function, 2.add read/write multiple table fun
/// </summary>
namespace Base.Services
{
    //single row delegate function
    //public delegate string AfterSave(Db db);

    //public delegate string WhenSave(bool isNew, JObject row);
    //public delegate string WhenDelete(string key);

    //multiple rows delegate function, delete first then update
    //public delegate string WhenSaves(bool isNew, JObject inputRow, List<List<string>> deleteKeys, JArray updateRows);
    //public delegate string WhenDeletes(string key);

    /// <summary>
    /// for Crud Edit Service
    /// </summary>
    public class CrudEdit
    {
        //constant
        //front end input json fields:
        private const string Rows = "_rows";        //multiple rows
        private const string Deletes = "_deletes";  //delete key string list
        private const string Childs = "_childs";    //child json list
        private const string FkeyFid = "_fkeyfid";  //foreign key fid
        private const string IsNew = "_isNew";      //crud field for is new row or not

        //master edit
        private EditDto _edit;
        private int _saveRows = 0;    //changed(new/edit) rows count

        //db str in config file
        private string _dbStr;
        //private object _dbBox;

        //sql args pair(fid,value)
        private List<object> _sqlArgs = new List<object>();

        //now time
        private DateTime _now;

        //new key json, format: t + levelStr = json, 
        //ex: t02 = { fxx = key1, fxx = key2}, xx is row index(base 1 !!)
        private JObject _newKeyJson = new JObject();

        //constructor
        //public CrudEdit(EditDto edit, string dbStr = "", object dbBox = null)
        public CrudEdit(EditDto edit, string dbStr = "")
        {
            //_localeNo = (localeNo >= 0) ? localeNo : _Fun.DefaultLocaleNo;
            //_session = session;
            _edit = edit;
            _dbStr = dbStr;
            //_dbBox = dbBox;
        }

        private Db GetDb()
        {
            return new Db(_dbStr);
        }

        //add argument into _argFids, _argValues
        private void AddArg(string fid, object value)
        {
            _sqlArgs.Add(fid);
            _sqlArgs.Add(value);
        }

        //clear argument
        private void ResetArg()
        {
            _sqlArgs = new List<object>();
        }

        //set _now
        private void SetNow()
        {
            _now = DateTime.Now;
        }

        /*
        //get pkey value list(sep with "," if multi keys) for update row
        //single pkey get Kid field, multi pkey get _key field !!
        //if no key value, then log
        private string GetUpdateKeyList(EditModel edit, JObject row)
        {
            try
            {
                //no matter single/multi pkey, read _key first if existed !!
                if (row[_Fun.Key] != null)
                    return row[_Fun.Key].ToString();

                //single pkey, read Kid
                if (!_Str.IsEmpty(edit.Kid))
                    return row[edit.Kid].ToString();

                //multi pkey
                var keys = "";
                foreach (var kid in edit.Kids)
                    keys += row[kid].ToString() + _Fun.ColSep;
                return (keys == "") ? "" : keys.Substring(0, keys.Length - 1);
            }
            catch
            {
                _Log.Error("CrudEdit.cs GetUpdateKeyList() failed: pkey value not existed: " + _Json.ToStr(row));
                return "";
            }
        }
        */

        //get where by pkey for query 1st table & updata tables, set sql args at the same time
        //for getRow & update
        private string GetWhereAndArg(EditDto edit, string key)
        {
            //kid 參數前加底線才不會跟update的欄位衝突
            var kid = "_" + edit.PkeyFid;  
            AddArg(kid, key);
            return edit.PkeyFid + "=@" + kid;

            /*
            //multi pkey
            var where = "";
            var and = "";
            //var json = _Json.StrToJson(key);
            var kids = edit.Kids;
            var keys = key.Split(_Fun.ColSep);
            for (var i = 0; i< kids.Length; i++)
            {
                //check pkey existed

                //var kid = kids[i];
                var kid2 = "_" + kids[i];
                AddArg(kid2, keys[i]);
                where += and + kids[i] + "=@" + kid2;
                and = " And ";
            }
            return where;
            */
        }

        //get select sql 
        private string GetSql(EditDto edit, string key)
        {
            ResetArg();
            var where = GetWhereAndArg(edit, key);
            return GetSqlByWhere(edit, where);
        }

        private string GetSqlByField(EditDto edit, string key)
        {
            return _Str.CheckKeyRule(key, edit.ReadSql)
                 ? string.Format(edit.ReadSql, key)
                 : "";
        }

        private string GetSqlByWhere(EditDto edit, string where)
        {
            //add columns list
            var list = "";
            foreach (var item in edit.Items)
                list += (item.Col == "" ? item.Fid : (item.Col + " as " + item.Fid)) + ",";

            list = list.Substring(0, list.Length - 1);

            //get sql
            var order = string.IsNullOrEmpty(edit.OrderBy) ? "" : " Order By " + edit.OrderBy;
            //var where = GetWhereWithArg(edit, key);
            return "Select " + list + " From " + edit.Table + " Where " + where + order;
        }

        //get key value of row
        public string GetKey(EditDto edit, JObject row)
        {
            return row[edit.PkeyFid].ToString();
        }

        /// <summary>
        /// has _hideKey for CSRF issue, check this field before update row
        /// </summary>
        /// <param name="key"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public JObject GetDbRow(EditDto edit, string key, Db db = null)
        {
            //reset sqlArgs first
            //ResetArg();

            //connect db if need
            var emptyDb = (db == null);
            if (emptyDb)
                db = GetDb();

            //return row & close db if need
            var sql = string.IsNullOrEmpty(edit.ReadSql)
                ? GetSql(edit, key)
                : GetSqlByField(edit, key);
            var row = db.GetJson(sql, _sqlArgs);
            if (emptyDb)
                db.Dispose();
            return row;
        }

        /// <summary>
        /// get rows for multi tables (1 to many)
        /// 包含: 多個欄位、_childs
        /// note: 1.master table must relat to child table
        /// </summary>
        /// <param name="key">table primary key value</param>
        /// <returns></returns>
        public JObject GetJson(string key)
        {
            var db = GetDb();
            var data = GetDbRow(_edit, key, db);    //return data
            if (data == null)
                goto lab_exit;

            //get child rows (recursive)
            var editChilds = _edit.Childs;
            if (editChilds != null && editChilds.Length > 0)
            {
                var childs = new JArray();
                var keys = new List<string>() { key };
                for (var i = 0; i < editChilds.Length; i++)
                    childs.Add(GetChildDbJson(1, editChilds[i], keys, db));
                data[Childs] = childs;
            }
            
        lab_exit:
            db.Dispose();
            return data;
        }

        /// <summary>
        /// get childs rows(json) from db (recursive)
        /// 傳回JObject, 最多包含2個欄位: _rows, _childs
        /// </summary>
        /// <param name="editLevel">base 0, 傳入值會從1開始</param>
        /// <param name="edit"></param>
        /// <param name="keys"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private JObject GetChildDbJson(int editLevel, EditDto edit, List<string> keys, Db db)
        {
            //get this rows
            var keyList = _List.ToStr(keys, true);
            var sql = string.IsNullOrEmpty(edit.ReadSql)
                ? GetSqlByWhere(edit, edit.FkeyFid + " in (" + keyList + ")")
                : GetSqlByField(edit, keyList);
            var rows = db.GetJsons(sql);
            if (rows == null)
                return null;

            //prepare return data
            var data = new JObject() { [Rows] = rows };

            //get childs json list(recursive)
            var editChilds = edit.Childs;
            if (editChilds != null && editChilds.Length > 0)
            {
                keys = _Json.ArrayToListStr(rows, edit.PkeyFid);
                var childs = new JArray();
                for (var i = 0; i < editChilds.Length; i++)
                    childs.Add(GetChildDbJson(editLevel + 1, editChilds[i], keys, db));
                data[Childs] = childs;
            }
            return data;

            /*
            //get childRowsList: childs rows for child
            keys = _Json.ArrayToListStr(rows, edit.Kid);
            var childRowsList = new List<JArray>();
            var childLen = editChilds.Count;
            for (var i = 0; i < childLen; i++)
                childRowsList.Add(GetChildJson(editLevel + 1, editChilds[i], keys, db));

            //filter childRowsList and add into return rows
            for (var i=0; i< rows.Count; i++)
            {
                var upRow = rows[i];
                var key = upRow[edit.Kid].ToString();
                var childs = new JArray();
                for (var j = 0; j < childLen; j++)
                {
                    //var childRows = ;
                    childs.Add(new JObject()
                    {
                        [Rows] = _Json.FindArray(childRowsList[j], editChilds[j].MapFid, key),
                    });
                }
                upRow[Childs] = childs;            
            }
            return rows;
            */
        }

        /*
        private JArray GetRowsOfJson(JObject json)
        {
            return (json == null || json[Rows] == null)
                ? null
                : json[Rows] as JArray;
        }
        */

        private int GetEditChildLen(EditDto edit)
        {
            return (edit.Childs == null) ? 0 : edit.Childs.Length;
        }

        /// <summary>
        /// get json rows
        /// </summary>
        /// <param name="json">input row</param>
        /// <returns>JArray</returns>
        public JArray GetJsonRows(JObject json)
        {
            if (json == null || json[Rows] == null)
                return null;

            var rows = json[Rows] as JArray;
            return (rows.Count == 0)
                ? null
                : rows;
        }

        /// <summary>
        /// get child rows from upJson
        /// </summary>
        /// <param name="upJson">input row</param>
        /// <param name="childIdx">child index</param>
        /// <returns>JArray</returns>
        public JArray GetChildRows(JObject upJson, int childIdx)
        {
            var child = GetChildJson(upJson, childIdx);
            return (child == null || child[Rows] == null)
                ? null
                : child[Rows] as JArray;
        }

        /// <summary>
        /// get child json from upJson
        /// </summary>
        /// <param name="upJson"></param>
        /// <param name="childIdx"></param>
        /// <returns></returns>
        public JObject GetChildJson(JObject upJson, int childIdx)
        {
            if (upJson == null || upJson[Childs] == null)
                return null;

            //JArray childs = upJson[Childs] as JArray;
            return (upJson[Childs].Count() <= childIdx
                    || _Json.IsEmpty(upJson[Childs][childIdx] as JObject))
                ? null
                : upJson[Childs][childIdx] as JObject;
        }

        /*
        //get field id & index mapping
        //return new JObject() when null for make coding easy !!
        private void SetFidNo(EditDto edit)
        {
            edit._FidNo = new JObject();
            var fidNo = edit._FidNo;
            var items = edit.Items;
            for(var i=0; i< items.Length; i++)
                fidNo[items[i].Fid] = i;
        }

        //set key value and map key value(recursive)
        //return status
        private bool SetKeyAndMap(EditDto edit, JObject inputRow)
        {
            if (inputRow[edit.Kid] == null)
                return false;

            var childLen = GetChildLen(edit);
            for (var i = 0; i < childLen; i++)
            {
                var edit2 = edit.Childs[i];
                JArray rows = GetInputChildRows(inputRow, i);
                if (rows != null && rows.Count > 0)
                {
                    //recursive insert rows
                    if (!InsertRows(edit2, key, rows, db))
                        return false;
                }
            }
        }
        */

        /// <summary>
        /// get primary key(value)
        /// </summary>
        /// <param name="edit"></param>
        /// <param name="row"></param>
        /// <returns>0 if not new row</returns>
        /*
        private int ParsePkey(EditDto edit, JObject row)
        {
            return ParseCol(row, edit.PkeyFid);
        }
        */

        //parse foreign key
        private int ParseFkey(JObject row)
        {
            return ParseCol(row, FkeyFid);
        }

        /// <summary>
        /// parse column for PkCol, FkCol
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns>-1(error), 0(right key), n(new key)</returns>
        private int ParseCol(JObject row, string col)
        {
            return _Str.IsEmpty(row[col]) ? -1 :
                Int32.TryParse(row[col].ToString(), out int num) ? num : 
                0;
        }

        /// <summary>
        /// check is new key or not by kid
        /// </summary>
        /// <param name="row"></param>
        /// <param name="kid"></param>
        /// <returns></returns>
        private bool IsNewKey(JObject row, string kid)
        {
            return ParseCol(row, kid) > 0;
        }

        /// <summary>
        /// check is new row or not by IsNew field
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private bool IsNewRow(JObject row)
        {
            return (row[IsNew] != null && row[IsNew].ToString() == "1");
        }

        /*
        private bool SetKeyAndMaps(List<string> upKeys, EditDto edit, JArray rows)
        {
            var keys = new List<string>();
            if (rows != null)
            {
                foreach(JObject row in rows)
                {
                    //set key
                    if (row[edit.Kid] != null)
                    {
                        //set new key if need
                        var keyIdx = GetNewKeyIdx(row[edit.Kid].ToString());
                        if (keyIdx > 0)
                            row[edit.Kid] = _Str.NewId();

                        //key
                    }
                    if (row)
                    //set map key
                }
            }

            if (inputRow[edit.Kid] == null)
                return false;

            var childLen = GetChildLen(edit);
            for (var i = 0; i < childLen; i++)
            {
                var edit2 = edit.Childs[i];
                JArray rows = GetInputChildRows(inputRow, i);
                if (rows != null && rows.Count > 0)
                {
                    //recursive insert rows
                    if (!InsertRows(edit2, key, rows, db))
                        return false;
                }
            }
        }
        */

        private bool HasField(JObject row, string kid)
        {
            if (row == null)
                return false;

            foreach (var field in row)
            {
                //skip under line field
                var fid = field.Key;
                if (fid != kid && !IsSpecEditFid(fid))
                    return true;
            }

            //case of none
            return false;
        }

        //是否為特殊編輯欄位
        private bool IsSpecEditFid(string fid)
        {
            return (fid.Substring(0, 1) == "_");
        }

        /// <summary>
        /// insert one row
        /// </summary>
        /// <param name="edit"></param>
        /// <param name="inputRow"></param>
        /// <param name="db"></param>
        /// <returns>status</returns>
        private bool InsertRow(EditDto edit, JObject inputRow, Db db)
        {
            //set key & map field

            #region insert row if need
            //reset sqlArgs first
            ResetArg();

            /*
            //set new key
            string key;
            if (edit.AutoNewId)
            {
                key = _Str.NewId();
                inputRow[edit.PkeyFid] = key;
            }
            else
            {
                key = inputRow[edit.PkeyFid].ToString();
            }
            */

            //set default value
            edit.Items
                .Where(a => a.Value != null)
                .ToList()
                .ForEach(a =>
                {
                    inputRow[a.Fid] = a.Value.ToString();
                });

            //prepare sql
            var fids = "";
            var values = "";
            var userId = _Fun.UserId();   //get userId from child class
            foreach (var field in inputRow)
            {
                //skip under line field
                var fid = field.Key;
                if (IsSpecEditFid(fid))
                    continue;

                //如果傳入欄位不存在, 則log error !!
                if (edit._FidNo[fid] == null)
                {
                    _Log.Error("CrudEdit.cs InsertRow() field not existed(" + edit.Table + "." + fid + ")");
                    return false;
                }

                //skip not created field
                var itemNo = Convert.ToInt32(edit._FidNo[fid]);
                if (!edit.Items[itemNo].Create)
                    continue;

                //get value and check EmptyToNulls
                var value = (inputRow[fid].ToString() == "" && edit.EmptyToNulls.Contains(fid))
                    ? null
                    : inputRow[fid].ToString();

                //add keys & values
                fids += fid + ",";
                values += "@" + fid + ",";
                AddArg(fid, value);
            }

            //return false if no fields
            if (fids == "")
            {
                _Log.Error("CrudEdit.cs InsertRow() fields are empty.");
                return false;
            }

            //set creator, created if need
            //var setCol4 = "";
            if (edit.Col4 != null && edit.Col4.Length >= 2)
            {
                var fldUser = edit.Col4[0];
                var fldDate = edit.Col4[1];
                if (fldUser != null && fldDate != null)
                {
                    fids += fldUser + "," + fldDate + ",";
                    values += string.Format("'{0}','{1}',", userId, _Date.ToDbStr(_now));
                }
                else if (fldUser != null)
                {
                    fids += fldUser + ",";
                    values += string.Format("'{0}',", userId);
                }
                else
                {
                    fids += fldDate + ",";
                    values += string.Format("'{0}',", _Date.ToDbStr(_now));
                }
            }

            //insert db
            var sql = "Insert Into " + edit.Table + 
                " (" + fids.Substring(0, fids.Length - 1) + ") Values (" + 
                values.Substring(0, values.Length - 1) + ")";
            if (db.ExecSql(sql, _sqlArgs) == 0)
                return false;
            #endregion

            /*
            #region insert/update childs rows
            var key = GetKey(edit, inputRow);
            var childLen = GetChildLen(edit);
            for (var i = 0; i < childLen; i++)
            {
                var edit2 = edit.Childs[i];
                JArray rows = GetInputChildRows(inputRow, i);
                if (rows != null && rows.Count > 0)
                {
                    //recursive insert rows
                    if (!InsertRows(edit2, key, rows, db))
                        return false;
                }
            }
            #endregion
            */

            //case of ok
            _saveRows++;
            return true;
        }

        /*
        //insert rows, recursive(call InsertRow)!!
        private bool InsertRows(EditDto edit, string upKey, JArray rows, Db db)
        {
            if (rows == null || rows.Count == 0)
                return true;

            //loop: call InsertRow()
            foreach (JObject row in rows)
            {
                //set Mapping column value
                row[edit.MapId] = upKey;

                //insert this row
                if (!InsertRow(edit, row, db))
                    return false;
            }

            //case of ok
            return true;
        }
        */

        //update rows, recursive!!
        //upData: 包含 _rows, _childs, _deletes(字串list)
        //步驟: 1.set key, 2.delete sub, 3.insert/update
        //return error msg if any

        /*
        //mapKey的內容為數字
        //mapId字串長度小於10, 表示從keyJson讀取
        private string GetMapId(JObject keyJson, object mapId)
        {
            if (_Str.IsEmpty(mapId))
            {
                _Log.Error("CrudEdit.cs GetMapId() mapId is empty.");
                return "";
            }

            var mapId2 = mapId.ToString();  //to string
            if (mapId2.Length == 10)
                return mapId2;

            //get from up json
            var fid = "f" + mapId2;
            if (keyJson[fid] == null)
            {
                _Log.Error("CrudEdit.cs GetMapId() keyJson get empty value.");
                return "";
            }

            //case of ok
            return keyJson[fid].ToString();
        }
        */

        //update one row, recursive!!
        //return error msg if any
        private bool UpdateRow(EditDto edit, JObject inputRow, Db db)
        {
            //如果無任何異動欄位, 則不需處理
            if (_Json.IsEmpty(inputRow))
                return true;

            /* 不讀取 db, 直接 update
            #region get existed db row
            //var edit = _edit;
            var rowKey = inputRow[edit.Kid].ToString();
            var sql = GetSql(edit, rowKey);
            var dbRow = db.GetJson(sql, _sqlArgs);
            if (dbRow == null)
            {
                _Log.Error("CrudEdit.cs UpdateRow() found no row: " + sql + db.GetArgsText(_sqlArgs));
                return false;
            }
            #endregion
            */

            #region update this row
            //reset sql arguments first
            ResetArg();

            //get updated sql, compare db/input row
            var sql = "";
            var rowKey = inputRow[edit.PkeyFid].ToString();
            foreach (var field in inputRow)
            {
                //如果傳入欄位不存在, 則log error !!
                var fid = field.Key;
                if (IsSpecEditFid(fid))
                    continue;

                if (edit._FidNo[fid] == null)
                {
                    _Log.Error("CrudEdit.cs UpdateRow() field not existed(" + edit.Table + "." + fid + ")");
                    return false;
                }

                //key is not updated
                if (fid == edit.PkeyFid)
                    continue;

                //如果此欄位不更新, 或是與db內容相同, 則skip
                var fidNo = Convert.ToInt32(edit._FidNo[fid]);
                if (!edit.Items[fidNo].Update
                    //|| inputRow[fid] == null
                    //|| inputRow[fid].ToString() == dbRow[fid].ToString()
                    )
                    continue;

                //把空的日期欄位值轉成null if need, 否則會存入1900/1/1 !!
                //object value = (inputRow[key].ToString() == "" && (type == EnumDataType.Datetime || type == EnumDataType.Date))
                object value = (inputRow[fid].ToString() == "" && edit.EmptyToNulls.Contains(fid))
                    ? value = null
                    : inputRow[fid].ToString();

                //add into sql
                sql += fid + "=@" + fid + ",";
                AddArg(fid, value);
            }

            //set sql, emtpy sql means no column is changed !!
            if (sql == "")
                return true;

            //set reviser, revised
            var setCol4 = "";
            if (edit.Col4 != null && edit.Col4.Length == 4)
            {
                var fldUser = edit.Col4[2];
                var fldDate = edit.Col4[3];
                setCol4 = (fldUser != null && fldDate != null)
                    ? string.Format(",{0}='{1}',{2}='{3}'", fldUser, _Fun.UserId(), fldDate, _Date.ToDbStr(_now)) :
                (fldUser != null)
                    ? string.Format(",{0}='{1}'", fldUser, _Fun.UserId()) :
                (fldDate != null)
                    ? string.Format(",{0}='{1}'", fldDate, _Date.ToDbStr(_now)) : "";
            }

            //update db
            sql = "Update " + edit.Table + " Set " + sql.Substring(0, sql.Length - 1) + setCol4 + " Where " + GetWhereAndArg(edit, rowKey);
            //AddArg(edit.Kid, inputKey);
            if (db.ExecSql(sql, _sqlArgs) == 0)
                return false;

            //case of ok
            _saveRows++;
            return true;
            #endregion

            /*
            #region update/insert childs rows
            //update childs rows
            var key = GetKey(edit, inputRow);
            var childLen = GetChildLen(edit);
            for (var i = 0; i < childLen; i++)
            {
                var status = true;
                var edit2 = edit.Childs[i];
                List<string> deletes = (inputRow[Deletes] == null) ? null : _Str.ToList(inputRow[Deletes].ToString());
                JArray childRows = (inputRow[Rows] == null) ? null : (JArray)inputRow[Rows];

                //adjust by parent if need
                //if (edit.DeleteThenInsert)
                //    edit2.DeleteThenInsert = true;

                //delete rows, consider delete then insert
                //if (edit2.DeleteThenInsert)
                //    status = DeleteRowsByUpKey(edit2, key, db);
                else if (deletes != null && deletes.Count > 0)
                    status = DeleteRowsByKeys(edit2, deletes, db);

                //insert & update
                if (status && childRows != null)
                {
                    foreach (JObject row2 in childRows)
                    {
                        if (IsNewRow(edit2, row2))
                        {
                            row2[edit2.MapId] = key; //set key value
                            status = InsertRow(edit2, row2, db);
                        }
                        else
                        {
                            status = UpdateRow(edit2, row2, db);
                        }
                        if (!status)
                            break;
                    }
                }

                //master table must be single
                if (!status)
                    return false;
            }
            #endregion

            //case of ok
            return true;
            */
        }

        /*
        private List<string> GetItemRequires(EditDto edit)
        {
            //get required field list
            return edit.Items
                .Where(a => a.Required == true)
                .Select(a => a.Fid)
                .ToList();
        }
        */

        /// <summary>
        /// set edit validate variables: _FidNo, _FidRequires
        /// //(recursive)
        /// </summary>
        /// <param name="edit"></param>
        private void SetValidVar(EditDto edit)
        {
            //set this
            //_FidNo
            edit._FidNo = new JObject();
            var fidNo = edit._FidNo;
            var items = edit.Items;
            for (var i = 0; i < items.Length; i++)
                fidNo[items[i].Fid] = i;

            //_FidRequires
            edit._FidRequires = edit.Items
                .Where(a => a.Required == true)
                .Select(a => a.Fid)
                .ToList();

            /*
            //set childs(recursive)
            var childLen = GetEditChildLen(edit);
            for (var i = 0; i < childLen; i++)
                SetValidVar(edit.Childs[i]);
            */
        }

        /// <summary>
        /// validate json (recursive)
        /// </summary>
        /// <param name="editLevel">for debug</param>
        /// <param name="edit"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        private bool ValidJson(int editLevel, EditDto edit, JObject json)
        {
            //validate this
            JArray rows = GetJsonRows(json);
            if (rows != null)
            {
                //prepare edit validate variables
                SetValidVar(edit);

                foreach(var row in rows)
                {
                    if (row == null)
                        continue;
                    if (!ValidRow(edit, row as JObject))
                        return false;
                }
            }

            //validate this
            if (!ValidRow(edit, json))
                return false;

            //validate childs (recursive)
            var childLen = GetEditChildLen(edit);
            for(var i=0; i<childLen; i++)
            {
                var json2 = GetChildJson(json, i);
                if (json2 != null && !ValidJson(editLevel+1, edit.Childs[i], json2))
                    return false;
            }

            //case of ok
            return true;
        }

        /*
        /// <summary>
        /// check childs rows, recursive
        /// </summary>
        /// <param name="edit"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        private bool ValidChilds(EditDto edit, JArray rows)
        {
            if (rows == null)
                return true;

            //validate this & childs
            var childLen = GetEditChildLen(edit);
            foreach (JObject row in rows)
            {
                if (row == null)
                    continue;

                //validate this
                if (!ValidRow(edit, row))
                    return false;

                //validate childs
                for (var i = 0; i < childLen; i++)
                    if (!ValidChilds(edit.Childs[i], GetChildRows(row, i)))
                        return false;
            }

            //case of ok
            return true;
        }
        */

        //validate one  row
        private bool ValidRow(EditDto edit, JObject row)
        {
            if (_Json.IsEmptyBySkipUnderLine(row))
                return true;

            #region check required & fid existed
            if (IsNewKey(row, edit.PkeyFid))
            {
                //check required
                foreach (var fid in edit._FidRequires)
                {
                    if (_Str.IsEmpty(row[fid]))
                    {
                        _Log.Error("field is required for insert.(" + edit.Table + "." + fid + ")");
                        return false;
                    }
                }
            }
            else
            {
                //check required
                foreach (var item in row)
                {
                    //底線欄位不檢查是否存在DB
                    var fid = item.Key;
                    if (IsSpecEditFid(fid))
                        continue;

                    //log error if fid not existed
                    if (edit._FidNo[fid] == null)
                    {
                        _Log.Error(string.Format("input field is wrong for {0}.({1})", edit.Table, fid));
                        return false;
                    }

                    //check required
                    if (_Str.IsEmpty(row[fid]) && edit._FidRequires.Contains(fid))
                    {
                        _Log.Error("field is required for update.(" + edit.Table + "." + fid + ")");
                        return false;
                    }
                }
            }
            #endregion

            #region field value validation by data type
            var typeName = "";
            try
            {
                foreach (var col in row)
                {
                    var fid = col.Key;
                    if (IsSpecEditFid(fid))
                        continue;

                    var value = row[fid].ToString();
                    var itemNo = Convert.ToInt32(edit._FidNo[fid]);
                    var item = edit.Items[itemNo];
                    switch (item.CheckType)
                    {
                        //TODO: add others checkType
                        case CheckTypeEstr.None:
                            continue;
                        case CheckTypeEstr.Email:
                            typeName = "Email";
                            if (!_Valid.IsEmail(value))
                            {
                                _Log.Error("not email: " + value);
                                return false;
                            }
                            break;
                        case CheckTypeEstr.Url:
                            typeName = "Url";
                            continue;
                        /*
                        case CheckTypeEstr.CreditCard:
                            typeName = "CreditCard";
                            continue;
                        case CheckTypeEstr.Digits:
                            typeName = "Digits";
                            if (!_Valid.IsDigits(value))
                            {
                                _Log.Error("not digits: " + value);
                                return false;
                            }
                            break;
                        case CheckTypeEstr.Number:
                            typeName = "Number";
                            if (!_Valid.IsNumber(value))
                            {
                                _Log.Error("not number: " + value);
                                return false;
                            }
                            break;
                        */
                        case CheckTypeEstr.Min:
                            typeName = "Min";
                            if (Convert.ToDecimal(value) > Convert.ToDecimal(item.CheckData))
                            {
                                _Log.Error("min not match: " + value + ", " + item.CheckData);
                                return false;
                            }
                            break;
                        case CheckTypeEstr.Max:
                            typeName = "Max";
                            if (Convert.ToDecimal(value) < Convert.ToDecimal(item.CheckData))
                            {
                                _Log.Error("max not match: " + value + ", " + item.CheckData);
                                return false;
                            }
                            break;
                        case CheckTypeEstr.Range:
                            typeName = "Range";
                            var values = item.CheckData.Split(',');
                            var value2 = Convert.ToDecimal(value);
                            if (value2 < Convert.ToDecimal(values[0]) || value2 > Convert.ToDecimal(values[1]))
                            {
                                _Log.Error("range not match: " + value + ", " + values[0] + ", " + values[1]);
                                return false;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _Log.Error("CrudEdit.cs CheckRow() failed: CheckType=" + typeName + ", msg=" + ex.Message);
                return false;
            }

            /*
            //check childs
            var childLen = GetChildLen(edit);
            for(var i=0; i<childLen; i++)
            {
                var edit2 = edit.Childs[i];
                if (!CheckRows(edit2, fidNo, null))
                    return false;
            }
            */

            //case of ok
            return true;
            #endregion
        }

        /// <summary>
        /// save one row with transaction
        /// </summary>
        /// <param name="key"></param>
        /// <param name="row"></param>
        /// <param name="afterSave">enable transaction if not null</param>
        /// <returns></returns>
        //public ErrorModel Save_old(JObject row, AfterSave afterSave = null)
        //{
        //    //var edit = _edit;
        //    var key = row[_Fun.DataKey].ToString();
        //    //var trans = (afterSave != null);    //transaction or not
        //    var childLen = (_edit.Childs == null) ? 0 : _edit.Childs.Count;
        //    var trans = (_edit.Transaction != null)
        //        ? _edit.Transaction.Value
        //        : (childLen > 0);
        //    var error = string.Empty;
        //    var isNew = _Str.IsEmpty(key);
        //    //check input row
        //    Db db = null;
        //    JObject fidNo = null;
        //    //if (!edit.AllCols)
        //    //{
        //        fidNo = GetItemMap(edit.Items);
        //        if (!CheckRow(isNew, edit, fidNo, row))
        //            goto lab_error;
        //    //}

        //    /*
        //    //custom check
        //    //var isNew = String.IsNullOrEmpty(inputKey);
        //    if (whenSave != null)
        //    {
        //        try
        //        {
        //            var error = whenSave(isNew, mainRow);
        //            if (error != "")
        //                return new ErrorModel() { ErrorMsg = error };
        //        }
        //        catch (Exception ex)
        //        {
        //            //log error
        //            _Log.Error("CrudEdit.cs Save() failed for WhenSave(): " + ex.Message);
        //            //error = _Msg.SaveFail + "(1)";
        //            goto lab_error;
        //        }
        //    }
        //    */

        //    SetNow();

        //    //get db
        //    db = GetDb();
        //    if (trans)
        //        db.BeginTran();

        //    //update db
        //    var status = isNew
        //        ? InsertRow(edit, fidNo, row, db)
        //        : UpdateRow(edit, fidNo, row, db);
        //    if (!status)
        //        goto lab_error;

        //    //call afterSave() if need
        //    if (afterSave != null)
        //    {
        //        //try
        //        //{
        //            error = afterSave(db);
        //            if (error != "")
        //                goto lab_error;
        //            /*
        //        }
        //        catch (Exception ex)
        //        {
        //            //log error
        //            _Log.Error("CrudEdit.cs Save() failed for AfterSave(): " + ex.Message);
        //            goto lab_error;
        //        }
        //        */
        //    }

        //    //case of ok
        //    if (trans)
        //        db.Commit();
        //    db.Dispose();
        //    return new ErrorModel();

        //    //case of error
        //    lab_error:
        //    if (db != null)
        //    {
        //        if (trans)
        //            db.Rollback();
        //        db.Dispose();
        //    }
        //    if (error == string.Empty)
        //        error = _Fun.SystemError;
        //    return new ErrorModel() { ErrorMsg = error };
        //}

        //is transaction or not
        private bool IsTrans(EditDto edit)
        {
            var childLen = GetEditChildLen(edit);
            return (_edit.Transaction != null)
                ? _edit.Transaction.Value
                : (childLen > 0);
        }

        /// <summary>
        /// save new rows, use transaction
        /// </summary>
        /// <param name="json"></param>
        /// <param name="fnAfterSave"></param>
        /// <returns>ResultDto</returns>
        public ResultDto Create(JObject json,
            FnSetNewKeyJson fnSetNewKey = null, FnAfterSave fnAfterSave = null)
        {
            return SaveJson("", json, fnSetNewKey, fnAfterSave);
        }

        /// <summary>
        /// save updated rows(including delete rows), use transaction
        /// </summary>
        /// <param name="key">key of master table</param>
        /// <param name="json"></param>
        /// <param name="fnAfterSave"></param>
        /// <returns>ResultDto</returns>
        public ResultDto Update(string key, JObject json,
            FnSetNewKeyJson fnSetNewKey = null, FnAfterSave fnAfterSave = null)
        {
            //return error if empty key
            if (string.IsNullOrEmpty(key))
                return new ResultDto()
                {
                    ErrorMsg = "CrudEdit.cs Update() failed: key is empty."
                };

            return SaveJson(key, json, fnSetNewKey, fnAfterSave);
        }

        /// <summary>
        /// save rows including delete rows, use transaction
        /// called by Create(), Update()
        /// </summary>
        /// <param name="inputJson">input json</param>
        /// <param name="fnSetNewKeyJson">custom function for set newKeyJson</param>
        /// <param name="fnAfterSave"></param>
        /// <returns></returns>
        private ResultDto SaveJson(string key, JObject inputJson, 
            FnSetNewKeyJson fnSetNewKeyJson = null, FnAfterSave fnAfterSave = null)
        {
            //check input & set fidNos same time
            Db db = null;
            var error = string.Empty;
            var trans = IsTrans(_edit);
            if (inputJson == null)
            {
                error = "input json is null";
                goto lab_error;
            }

            //var key = (json[_edit.PkeyFid] == null) ? "" : json[_edit.PkeyFid].ToString();
            //var isNew = _Str.IsEmpty(key);

            //check main row
            //SetFidNo(_edit);
            if (!ValidJson(0, _edit, inputJson))
            {
                error = "ValidJson() failed.";
                goto lab_error;
            }

            /*
            //check child rows, recursive
            if (!ValidChild(_edit, row))
                goto lab_error;

            //custom check
            //var error = "";
            if (whenSaves != null)
            {
                try
                {
                    var error = whenSaves(isNew, mainRow, deleteSubs, subRows);
                    if (error != "")
                        return new ErrorModel() { ErrorMsg = error };
                }
                catch (Exception ex)
                {
                    //log error
                    _Log.Error("CrudEdit.cs SaveRows() failed for WhenSaveRows(): " + ex.Message);
                    //error = _Msg.SaveFail + "(1)";
                    goto lab_error;
                }
            }
            //if (error != "")
            //    goto lab_error;
            */

            //set new key
            var status = (fnSetNewKeyJson == null)
                ? SetNewKeyJson(inputJson, _edit)
                : fnSetNewKeyJson(this, inputJson, _edit);
            if (!status)
            {
                error = "SetNewKey() failed.";
                goto lab_error;
            }

            //transaction if need
            db = GetDb();
            if (trans)
                db.BeginTran();

            //set current time(_now)
            SetNow();

            //改用 DB自行delete
            //var json = new JObject() { [Rows] = new JArray() { row } };
            if (!SaveJson2(_edit.HasFKey, "0", null, inputJson, _edit, db))
                goto lab_error;

            /*
            //update db
            var childLen = GetChildLen(_edit);
            if (isNew)
            {
                //insert row, recursive!!
                if (!InsertRow(_edit, row, db))
                    goto lab_error;
            }
            else
            {
                //update main row
                if (!UpdateRow(_edit, row, db))
                    goto lab_error;

                //update childs rows
                key = GetKey(_edit, row);
                for (var i = 0; i < childLen; i++)
                {
                    var status = true;
                    var edit2 = _edit.Childs[i];
                    List<string> deletes = (row[Deletes] == null) ? null : _Str.ToList(row[Deletes].ToString());
                    JArray childRows = (row[Rows] == null) ? null : (JArray)row[Rows];
                    //fidNo = fidNos[i] as JObject;
                    //var upKey = mainRow[edit.MapIds[0]].ToString();

                    //delete rows, consider delete then insert
                    //if (edit2.DeleteThenInsert)
                    //    status = DeleteRowsByUpKey(edit2, key, db);
                    else if (deletes != null && deletes.Count > 0)
                        status = DeleteRowsByKeys(edit2, deletes, db);

                    //insert & update
                    if (status && childRows != null)
                    {
                        foreach (JObject row2 in childRows)
                        {
                            if (IsNewRow(edit2, row2))
                            {
                                row2[edit2.MapId] = key; //set key value
                                status = InsertRow(edit2, row2, db);
                            }
                            else
                            {
                                status = UpdateRow(edit2, row2, db);
                            }
                            if (!status)
                                break;
                        }
                    }

                    //master table must be single
                    if (!status)
                        goto lab_error;
                }
            }
            */

            //call afterSave() if need
            if (fnAfterSave != null)
            {
                try
                {
                    error = fnAfterSave(db, _newKeyJson);
                    if (error != "")
                        goto lab_error;
                }
                catch (Exception ex)
                {
                    //log error
                    _Log.Error("CrudEdit.cs Saves() failed for AfterSave(): " + ex.Message);
                    goto lab_error;
                }
            }

            //case of ok
            if (trans)
                db.Commit();
            db.Dispose();
            return new ResultDto() { Value = _saveRows.ToString() };

        lab_error:
            if (db != null)
            {
                if (trans)
                    db.Rollback();
                db.Dispose();
            }

            if (error == string.Empty)
                return _Fun.GetSystemError();
            else
            {
                _Log.Error("CrudEdit Save() failed: " + error);
                return new ResultDto() { ErrorMsg = error };
            }
        }

        /// <summary>
        /// validate and save(recursive)
        /// </summary>
        /// <param name="hasFkey">table has foreign key or not</param>
        /// <param name="levelStr">level concat string, ex:0,00,012</param>
        /// <param name="upDeletes">empty for level0</param>
        /// <param name="edit"></param>
        /// <param name="inputJson">JObject for level0, JArray for level1/2</param>
        /// <param name="db"></param>
        /// <returns></returns>
        private bool SaveJson2(bool hasFkey, string levelStr, List<string> upDeletes, 
            JObject inputJson, EditDto edit, Db db)
        {
            if (inputJson == null)
                return true;

            var levelLen = levelStr.Length;
            var isLevel0 = (levelLen == 1);

            #region delete first & get deleted list for child(if need)
            List<string> deletes = (inputJson[Deletes] == null)
                ? null : _Str.ToList(inputJson[Deletes].ToString());
            if (deletes != null)
            {
                //deleted key, no special char !!
                if (isLevel0 && !_List.IsAlphaNum(deletes, "CrudEdit.cs SaveJson()"))
                    return false;

                //if no Fkey, use deleted key for child's upKey
                if (!hasFkey)
                    deletes = _List.Concat(deletes, GetKeysByUpKeys(edit, upDeletes, db));

                if (!DeleteRowsByKeys(edit, deletes, db))
                    return false;
            }
            #endregion

            #region insert/update this rows
            var inputRows = (inputJson[Rows] == null)
                ? null : inputJson[Rows] as JArray;
            JObject upNewKey2 = new JObject(); //new pkey for childs fkey
            if (inputRows != null)
            {
                var kid = edit.PkeyFid;
                foreach (var inputRow0 in inputRows)
                {
                    //inputRow0 could be null, save to var first, or will error
                    if (inputRow0 == null || !inputRow0.HasValues)
                        continue;

                    //insert/update this
                    var inputRow = inputRow0 as JObject;
                    if (!HasField(inputRow, kid))
                        continue;

                    if (IsNewRow(inputRow))
                    {
                        if (!InsertRow(edit, inputRow, db))
                            return false;
                    }
                    else
                    {
                        if (!UpdateRow(edit, inputRow, db))
                            return false;
                    }
                }//for rows
            }//if
            #endregion

            #region insert/update childs(recursive)
            var childLen = GetEditChildLen(edit);
            for (var i = 0; i < childLen; i++)
            {
                //recursive call
                var childJson = GetChildJson(inputJson, i);
                if (!SaveJson2(hasFkey, levelStr + i, deletes, childJson, edit.Childs[i], db))
                    return false;
            }//for childs
            #endregion

            //case of ok
            return true;
        }

        public JObject GetNewKeyJson()
        {
            return _newKeyJson;
        }

        /// <summary>
        /// get new key of master table
        /// </summary>
        /// <returns></returns>
        public string GetNewKey()
        {
            var fid = "t0";
            return (_newKeyJson[fid] == null) ? null : _newKeyJson[fid].ToString();
        }

        public bool SetNewKeyJson(JObject inputJson, EditDto edit)
        {
            return SetNewKeyJson2("0", null, inputJson, edit);
        }

        /// <summary>
        /// (recursive)set new key json(_newKeyJson), called by SetNewKey()
        /// </summary>
        /// <param name="levelStr">level concat string, ex:0,00,012</param>
        /// <param name="upNewKey">empty for level0, string for level1, JObject for level2...
        /// <param name="edit"></param>
        /// <param name="inputJson">JObject for level0, JArray for level1/2</param>
        /// <returns>status</returns>
        private bool SetNewKeyJson2(string levelStr, JObject upNewKey, JObject inputJson, EditDto edit)
        {
            if (inputJson == null)
                return true;

            var levelLen = levelStr.Length;
            var isLevel0 = (levelLen == 1);

            #region insert/update this rows
            var inputRows = (inputJson[Rows] == null)
                ? null : inputJson[Rows] as JArray;
            JObject upNewKey2 = new JObject(); //new pkey for childs fkey
            if (inputRows != null)
            {
                var kid = edit.PkeyFid;
                foreach (var inputRow0 in inputRows)
                {
                    //inputRow0 could be null, save to var first, or will error
                    if (inputRow0 == null || !inputRow0.HasValues)
                        continue;

                    //insert/update this
                    var inputRow = inputRow0 as JObject;
                    if (HasField(inputRow, kid))
                    {
                        //adjust pkeyIdx if need
                        var pkeyIdx = ParseCol(inputRow, kid);
                        if (pkeyIdx < 0)
                        {
                            //< 0 means empty pkey, main edit allows empty pkey
                            if (isLevel0 && inputRows.Count == 1)
                                pkeyIdx = 1;    //adjust, let it be new
                            else
                            {
                                _Log.Error("CrudEdit.cs SaveJson() failed: can not get PkeyFid (" + edit.PkeyFid + ")");
                                return false;
                            }
                        }

                        //case of insert row
                        if (pkeyIdx != 0)
                        {
                            #region set foreign key value for not level0
                            if (!isLevel0)
                            {
                                var fkeyIdx = ParseFkey(inputRow);
                                if (fkeyIdx < 0)
                                {
                                    if (levelLen == 2)
                                    {
                                        fkeyIdx = 1;    //adjust
                                    }
                                    else
                                    {
                                        _Log.Error("CrudEdit.cs SaveJson() failed: can not get FkeyFid (" + edit.FkeyFid + ")");
                                        return false;
                                    }
                                }

                                if (fkeyIdx == 0)
                                {
                                    inputRow[edit.FkeyFid] = inputRow[FkeyFid].ToString();
                                }
                                else if (upNewKey == null)
                                {
                                    _Log.Error("CrudEdit.cs SaveJson() failed: can not get MapFid (" + edit.FkeyFid + ")");
                                    return false;
                                }
                                else
                                {
                                    //now upKeyData is key json (levelLen >= 3)
                                    inputRow[edit.FkeyFid] = upNewKey["f" + fkeyIdx];
                                }
                            }
                            #endregion

                            //get new key
                            var key = _Str.NewId();
                            inputRow[kid] = key;
                            inputRow[IsNew] = "1";  //string

                            //set upKeyData for child 
                            upNewKey2["f" + pkeyIdx] = key;
                        }
                    }//if row has fields
                }//for rows
            }//if has rows
            #endregion

            #region set childs new key (recursive)
            var childLen = GetEditChildLen(edit);
            for (var i = 0; i < childLen; i++)
            {
                //recursive call
                var childJson = GetChildJson(inputJson, i);
                if (!SetNewKeyJson2(levelStr + i, upNewKey2, childJson, edit.Childs[i]))
                    return false;
            }
            #endregion

            //set instance variables
            _newKeyJson["t" + levelStr] = upNewKey2;

            //case of ok
            return true;
        }

        /// <summary>
        /// FnSetNewKey() can call.
        /// </summary>
        /// <param name="levelStr"></param>
        /// <returns></returns>
        public bool SetRelatId(JObject inputJson, int childIdx, string fid, string fromLevelStr)
        {
            //get child rows
            string error;
            var rows = GetChildRows(inputJson, childIdx);
            if (rows == null)
                return true;

            //get table json first by levelStr
            var jsonFid = "t" + fromLevelStr;
            var json = (_newKeyJson[jsonFid] == null)
                ? null : (JObject)_newKeyJson[jsonFid];

            foreach (var row0 in rows)
            {
                if (row0 == null || !row0.HasValues)
                    continue;

                //has row need set or not
                var row = row0 as JObject;
                var keyIndex = ParseCol(row, fid);
                if (keyIndex > 0)
                {
                    if (json == null)
                    {
                        error = $"no _newKeyJson[{jsonFid}]";
                        goto labError;
                    }

                    row[fid] = json["f" + keyIndex].ToString();
                }
            }

            //case ok ok
            return true;

        labError:
            _Log.Error("CrudEdit.cs SetRelatId() failed: " + error);
            return false;
        }

        /// <summary>
        /// delete row for single table
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ResultDto Delete(string key)
        {
            return DeleteByKeys(new List<string>() { key });
        }

        /// <summary>
        /// delete rows for multiple tables
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public ResultDto DeleteByKeys(List<string> keys)
        {
            //check input
            if (!_List.IsAlphaNum(keys, "CrudEdit"))
                return _Fun.GetSystemError();

            //transaction or not
            //var error = "";
            //var len = _edit.Length;
            //var trans = (len > 1);
            var trans = IsTrans(_edit);
            var db = GetDb();
            if (trans)
                db.BeginTran();

            //set current time(_now)
            SetNow();

            var json = new JObject() { [Deletes] = _List.ToStr(keys, false) };
            if (!SaveJson2(_edit.HasFKey, "0", null, json, _edit, db))
                goto lab_error;

            /*
            //delete child rows first
            var childLen = GetChildLen(_edit);
            for (var i = 0; i < childLen; i++)
            {
                foreach (var key in keys)
                {
                    if (!DeleteRowsByUpKey(_edit.Childs[i], key, db))
                        goto lab_error;
                }
            }

            //delete master row
            if (!DeleteRowsByKeys(_edit, keys, db))
                goto lab_error;
            */

            if (trans)
                db.Commit();
            db.Dispose();
            return new ResultDto();

        lab_error:
            if (trans)
                db.Rollback();
            db.Dispose();

            //if (error != "")
            //    _Log.Error(error);
            return _Fun.GetSystemError();
        }

        /// <summary>
        /// delete rows of one table, input pkey list
        /// </summary>
        /// <param name="edit"></param>
        /// <param name="keys">can be multi pkey value(consider seperator)</param>
        /// <param name="db"></param>
        /// <returns>status</returns>
        private bool DeleteRowsByKeys(EditDto edit, List<string> keys, Db db = null)
        {
            //check input
            if (keys == null || keys.Count == 0)
                return true;

            //return msg
            //var error = "";
            ResetArg();

            var emptyDb = (db == null);
            if (emptyDb)
                db = GetDb() ;

            //delete rows
            //var fid = "";
            var values = "";
            var kid = "";
            //if (edit.Kid != null)
            //{
                //=== case of single pkey ===
                //set sql args
                kid = edit.PkeyFid;
                for (var i = 0; i < keys.Count; i++)
                {
                    var fid = edit.PkeyFid + i;
                    AddArg(fid, keys[i].ToString());
                    values += "@" + fid + ",";
                }
            //}

            //update db
            var sql = string.Format(_Fun.DeleteRowsSql, edit.Table, kid, values.Substring(0, values.Length - 1));
            //var sqlArgs = _sqlArgs;
            var count = db.ExecSql(sql, _sqlArgs);
            //if (count == 0)
            //    goto lab_error;
            if (emptyDb)
                db.Dispose();

            //case of ok
            _saveRows += count;
            return true;

            /*
        lab_error:
            if (emptyDb)
                db.Dispose();
            return false;
            */
        }

        private List<string> GetKeysByUpKeys(EditDto edit, List<string> upKeys, Db db)
        {
            if (upKeys == null || upKeys.Count == 0)
                return null;

            var sql = string.Format("select {0} from {1} where {2} in ({3})", edit.PkeyFid, edit.Table, edit.FkeyFid, _List.ToStr(upKeys, true));
            return db.GetStrs(sql);
        }

        /*
        //delete sub table rows, pass parent key value
        //return error msg if any
        private bool DeleteRowsByUpKeys(EditDto edit, List<string> upKeys, Db db)
        {
            //check input
            if (upKeys == null || upKeys.Count == 0)
                return true;

            //ResetArg();

            //var mapId = edit.MapId;
            //AddArg(kid, key);

            //update db
            var sql = string.Format("delete {0} where {1} in ({2})", edit.Table, edit.MapFid, _List.ToStr(upKeys, true));
            var count = db.Update(sql);
            if (count == 0)
                goto lab_error;

            //if (emptyDb)
            //    db.Dispose();

            //case of ok
            _saveRows += count;
            return true;

        lab_error:
            //if (emptyDb)
            //    db.Dispose();
            return false;
        }
        */
        #region remark code

        /*
        //傳回where條件字串 for 查詢2st table, 同時設定sql參數
        //只有單鍵的情形
        private string GetSqlWhereFor2nd(EditCrud edit, string key)
        {
            var kid = edit.MapIds;
            AddArg(kid, key);
            return kid + "=@" + kid;
        }
        */

        /*
        //檢查傳入資料格式是否正確: Html, Text
        private bool CheckRowFormat(EditModel edit, JObject fidNo, JObject inputRow)
        {
            //if (edit.AllCols)
            //    return "";

            foreach (var item in inputRow)
            {
                var key = item.Key;
                if (fidNo[key] == null)
                    continue;

                var itemNo = Convert.ToInt32(fidNo[key]);
                if (edit.Items[itemNo].Type == EnumItemType.Html)
                {
                    if (!_Valid.IsHtml(inputRow[key]))
                    {
                        _Log.Error("Html input wrong: " + inputRow[key].ToString());
                        return false;
                        //return result;
                    }
                }
                else
                {
                    if (!_Valid.IsText(inputRow[key]))
                    {
                        _Log.Error("Text input wrong: " + inputRow[key].ToString());
                        return false;
                        //return result;
                    }
                }
            }
            return true;
        }
        */

        /*
        //(多筆)檢查傳入資料有無必填錯誤或是格式不正確
        private bool CheckRowFormats(EditModel edit, JObject fidNo, JArray inputRows)
        {
            if (inputRows == null)
                return true;

            foreach (var row in inputRows)
            {
                if (!CheckRowFormat(edit, fidNo, (JObject)row))
                    return false;
            }
            return true;
        }
        */
        #endregion

    }//class
}
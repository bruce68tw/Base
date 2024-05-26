using Base.Enums;
using Base.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// 1.remove cache function
/// 2.add read/write multiple table fun
/// </summary>
namespace Base.Services
{
    /// <summary>
    /// for Crud Edit Service
    /// 讀取單筆資料時(Update/View), 傳回row、_childs
    /// </summary>
    public class CrudEditSvc : CrudEditGetSvc
    {
        //constant
        //front end input json fields:
        //private const string Rows = "_rows";        //multiple rows
        private const string Deletes = "_deletes";  //delete key string list
        //private const string Childs = "_childs";    //child json list
        private const string FkeyFid = "_fkeyfid";  //foreign key fid, 欄位內容如果是數字表示必須參考上一層key值
        private const string IsNew = "_isNew";      //crud field for is new row or not

        //master edit
        //private readonly EditDto _editDto;
        //private readonly string _ctrl;       //controll name
        private int _saveRows = 0;  //changed(new/edit) rows count

        //db str in config file
        //private readonly string _dbStr;

        //sql args pair(fid,value)
        //private List<object> _sqlArgs = new();

        private string _key = "";
        private bool _isNew = true;

        //now time
        private DateTime _now;

        //new key json, format: t + levelStr = json, 
        //ex: t02 = { fxx = key1, fxx = key2}, xx is row index(base 1 !!)
        private JObject _newKeyJson = new();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="editDto"></param>
        /// <param name="dbStr"></param>
        public CrudEditSvc(string ctrl, EditDto editDto, string dbStr = "")
            : base(ctrl, editDto, dbStr)
        {
            _ctrl = ctrl;
            _editDto = editDto;
            _dbStr = dbStr;
        }

        //set _now
        private void SetNow()
        {
            _now = DateTime.Now;
        }

        /*
        //get key value of row
        public string GetKey(EditDto edit, JObject row)
        {
            return row[edit.PkeyFid].ToString();
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
        private JArray? JsonToRows(JObject json)
        {
            if (json[_Fun.FidRows] == null) return null;
            var rows = json[_Fun.FidRows] as JArray;
            return (rows!.Count == 0)
                ? null : rows;
        }

        /// <summary>
        /// get child rows from upJson
        /// </summary>
        /// <param name="upJson">input row</param>
        /// <param name="childIdx">child index</param>
        /// <returns>JArray</returns>
        private JArray? GetChildRows(JObject upJson, int childIdx)
        {
            return _Json.GetChildRows(upJson, childIdx);
            /*
            var child = GetChildJson(upJson, childIdx);
            return (child == null || child[Rows] == null)
                ? null
                : child[Rows] as JArray;
            */
        }

        /// <summary>
        /// get child json from upJson
        /// </summary>
        /// <param name="upJson"></param>
        /// <param name="childIdx"></param>
        /// <returns></returns>
        private JObject? GetChildJson(JObject upJson, int childIdx)
        {
            return _Json.GetChildJson(upJson, childIdx);
            /*
            if (upJson == null || upJson[Childs] == null)
                return null;

            //JArray childs = upJson[Childs] as JArray;
            return (upJson[Childs].Count() <= childIdx
                    || _Json.IsEmpty(upJson[Childs][childIdx] as JObject))
                ? null
                : upJson[Childs][childIdx] as JObject;
            */
        }

        //parse foreign key
        private int GetFkeyIndex(JObject row)
        {
            return GetKeyIndex(row, FkeyFid);
        }

        //get master table key for update
        public string GetMainKey()
        {
            return _key;
        }

        /// <summary>
        /// for Pkey/Fkey, 讀取欄位的資料序號, 新增資料時, Key欄位會存入上一層的資料序號(前端傳入), 在後端寫入
        /// 如果欄位內容不為數字則表示為正常key值(傳回0)
        /// </summary>
        /// <param name="row"></param>
        /// <param name="kid">key field id</param>
        /// <returns>-1(error/not found), 0(正常key值), n(new key)</returns>
        private int GetKeyIndex(JObject row, string kid)
        {
            return _Object.IsEmpty(row[kid]) ? -1 :
                Int32.TryParse(row[kid]!.ToString(), out int num) ? num : 
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
            return GetKeyIndex(row, kid) < 0;
        }

        /// <summary>
        /// check is new row or not by IsNew field
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private bool IsNewRow(JObject row)
        {
            return (row[IsNew] != null && row[IsNew]!.ToString() == "1");
        }

        /// <summary>
        /// 檢查是否存在任何欄位(不含特殊欄位和主鍵欄位)
        /// </summary>
        /// <param name="row"></param>
        /// <param name="kid">主鍵欄位</param>
        /// <returns></returns>
        private bool HasInputField(JObject? row, string kid)
        {
            if (row == null) return false;

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

        //is special fid or not
        private bool IsSpecEditFid(string fid)
        {
            return (fid[..1] == "_");
        }

        /// <summary>
        /// insert one row
        /// </summary>
        /// <param name="edit"></param>
        /// <param name="inputRow"></param>
        /// <param name="db"></param>
        /// <returns>error msg if any</returns>
        private async Task<bool> InsertRowA(EditDto edit, JObject inputRow, Db db)
        {
            if (edit.Items == null || edit.Items.Length == 0) return true;

            #region insert row if need
            var error = "";            
            ResetArg();     //reset sqlArgs first

            //set default value
            edit.Items
                .Where(a => a.Value != null)
                .ToList()
                .ForEach(a =>
                {
                    inputRow[a.Fid] = a.Value!.ToString();
                });

            //prepare sql
            var fids = "";
            var values = "";
            var userId = _Fun.UserId();   //get userId from child class
            foreach (var field in inputRow)
            {
                //skip under line field
                var fid = field.Key;
                if (IsSpecEditFid(fid)) continue;

                //if no fid then log error !!
                if (edit._FidNo![fid] == null)
                {
                    error = $"CrudEdit.cs InsertRow() field not existed({edit.Table}.{fid})";
                    goto lab_error;
                }

                //skip not created field
                var itemNo = Convert.ToInt32(edit._FidNo[fid]);
                var eitem = edit.Items[itemNo];
                if (eitem.Read || !eitem.Create) continue;

                //get value and check EmptyToNulls
                var value = (inputRow[fid]!.ToString() == "" && edit.EmptyToNulls.Contains(fid))
                    ? null : inputRow[fid]!.ToString();

                //add keys & values
                fids += fid + ",";
                values += "@" + fid + ",";
                AddArg(fid, value!);
            }

            //return false if no fields
            if (fids == "")
            {
                error = "CrudEdit.cs InsertRow() fields are empty.";
                goto lab_error;
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
                    values += $"'{userId}','{_Date.ToDbStr(_now)}',";
                }
                else if (fldUser != null)
                {
                    fids += fldUser + ",";
                    values += $"'{userId}',";
                }
                else
                {
                    fids += fldDate + ",";
                    values += $"'{_Date.ToDbStr(_now)}',";
                }
            }

            //insert db
            var sql = $"Insert Into {edit.Table} ({fids[0..^1]}) Values ({values[0..^1]})";
            if (await db.ExecSqlA(sql, _sqlArgs!) == 0)
            {
                //not log error here, Db.cs already log it.
                //error = "CrudEdit.cs InsertRow() failed, insert no row.(" + sql + ")";
                goto lab_error;
            }
            #endregion

            //case of ok
            _saveRows++;
            return true;

        lab_error:
            if (_Str.NotEmpty(error)) await _Log.ErrorRootA(error);
            return false;
                
        }

        //update one row
        //return error msg if any
        private async Task<bool> UpdateRowA(bool isLevel0, EditDto edit, JObject inputRow, Db db)
        {
            //skip if empty
            if (_Json.IsEmpty(inputRow)) return true;

            /* not read db, just update
            #region get existed db row
            //var edit = _edit;
            var rowKey = inputRow[edit.PkeyFid].ToString();
            var sql = GetSql(edit, rowKey);
            var dbRow = db.GetRow(sql, _sqlArgs);
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
            var rowKey = isLevel0 ? GetMainKey() : inputRow[edit.PkeyFid]!.ToString();
            foreach (var field in inputRow)
            {
                //if no fid then log error !!
                var fid = field.Key;
                if (IsSpecEditFid(fid))
                    continue;

                if (edit._FidNo![fid] == null)
                {
                    await _Log.ErrorRootA($"CrudEdit.cs UpdateRowA() field not existed({edit.Table}.{fid})");
                    return false;
                }

                //key is not updated
                if (fid == edit.PkeyFid)
                    continue;

                //skip un-update fid
                var fidNo = Convert.ToInt32(edit._FidNo[fid]);
                var eitem = edit.Items[fidNo];
                if (eitem.Read || !eitem.Update)
                    continue;

                /* old code
                if (!edit.Items[fidNo].Update
                    //|| inputRow[fid] == null
                    //|| inputRow[fid].ToString() == dbRow[fid].ToString()
                    )
                    continue;
                */

                //set empty date to null, or will be 1900/1/1 !!
                //object value = (inputRow[key].ToString() == "" && (type == EnumDataType.Datetime || type == EnumDataType.Date))
                object? value = (inputRow[fid]!.ToString() == "" && edit.EmptyToNulls.Contains(fid))
                    ? null
                    : inputRow[fid]!.ToString();

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
                    ? $",{fldUser}='{_Fun.UserId()}',{fldDate}='{_Date.ToDbStr(_now)}'" :
                (fldUser != null)
                    ? $",{fldUser}='{_Fun.UserId()}'" :
                (fldDate != null)
                    ? $",{fldDate}='{_Date.ToDbStr(_now)}'" : "";
            }

            //update db
            sql = $"Update {edit.Table} Set {sql[0..^1] + setCol4} Where {GetWhereAndArg(edit, rowKey)}";
            if (await db.ExecSqlA(sql, _sqlArgs!) == 0)
            {
                await _Log.ErrorRootA($"CrudEdit.cs UpateRowA() failed, update 0 row.({sql})");
                return false;
            }

            //case of ok
            _saveRows++;
            return true;
            #endregion
        }

        /// <summary>
        /// set edit validate variables: _FidNo, _FidRequires
        /// </summary>
        /// <param name="edit"></param>
        private void SetValidVar(EditDto edit)
        {
            var items = edit.Items;
            if (items == null || items.Length == 0) return;

            //_FidNo
            edit._FidNo = new JObject();
            var fidNo = edit._FidNo;
            for (var i = 0; i < items.Length; i++)
                fidNo[items[i].Fid] = i;

            //_FidRequires
            edit._FidRequires = edit.Items
                .Where(a => a.Required == true)
                .Select(a => a.Fid)
                .ToList();
        }

        /// <summary>
        /// validate json (recursive)
        /// </summary>
        /// <param name="editLevel">for debug</param>
        /// <param name="edit"></param>
        /// <param name="json"></param>
        /// <returns>error msg if any</returns>
        private string ValidJsonLoop(int editLevel, EditDto edit, JObject json)
        {
            //if (json == null) return "";

            //validate this rows
            string error;
            JArray? rows = JsonToRows(json);
            if (rows != null)
            {
                //prepare edit validate variables
                SetValidVar(edit);

                //master-detail 類型時 master無異動時 rows[0]為null, 這裡必須使用var row, 不可用JObject row
                foreach(var row in rows)
                {
                    if (row == null) continue;
                    error = ValidRow(edit, (row as JObject)!);
                    if (error != "") return error;
                }
            }

            //validate this
            error = ValidRow(edit, json);
            if (error != "") return error;

            //validate childs (recursive)
            var childLen = GetEditChildLen(edit);
            for(var i=0; i<childLen; i++)
            {
                var json2 = GetChildJson(json, i);
                if (json2 == null) continue;

                error = ValidJsonLoop(editLevel + 1, edit.Childs![i], json2);
                if (error != "") return error;
            }            
            return "";  //case of ok
        }

        //validate one  row
        //return error msg if any
        private string ValidRow(EditDto edit, JObject row)
        {
            if (edit.Items == null || edit.Items.Length == 0) return "";
            if (_Json.IsEmptyNoSpec(row)) return "";

            #region check required & fid existed
            if (IsNewKey(row, edit.PkeyFid))
            {
                //check required
                if (edit._FidRequires != null)
                {
                    foreach (var fid in edit._FidRequires)
                    {
                        if (_Object.IsEmpty(row[fid]))
                            return "field is required for insert.(" + edit.Table + "." + fid + ")";
                    }
                }
            }
            else
            {
                //check required
                var  required = edit._FidRequires != null;
                foreach (var item in row!)
                {
                    //底線欄位不檢查是否存在DB
                    var fid = item.Key;
                    if (IsSpecEditFid(fid)) continue;

                    //log error if fid not existed
                    if (edit._FidNo![fid] == null)
                        return string.Format("input field is wrong ({0}.{1})", edit.Table, fid);

                    //check required
                    if (required && _Object.IsEmpty(row[fid]) && edit._FidRequires!.Contains(fid))
                        return "field is required for update.(" + edit.Table + "." + fid + ")";
                }
            }
            #endregion

            #region field value validation by data type
            var typeName = "";
            try
            {
                foreach (var col in row!)
                {
                    var fid = col.Key;
                    if (IsSpecEditFid(fid)) continue;

                    var value = row[fid]!.ToString();
                    var itemNo = Convert.ToInt32(edit._FidNo![fid]);
                    var item = edit.Items[itemNo];
                    switch (item.CheckType)
                    {
                        //TODO: add others checkType
                        case CheckTypeEstr.None:
                            continue;
                        case CheckTypeEstr.Email:
                            typeName = "Email";
                            if (!_Valid.IsEmail(value))
                                return "not email: " + value;
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
                                return "min not match: " + value + ", " + item.CheckData;
                            break;
                        case CheckTypeEstr.Max:
                            typeName = "Max";
                            if (Convert.ToDecimal(value) < Convert.ToDecimal(item.CheckData))
                                return "max not match: " + value + ", " + item.CheckData;
                            break;
                        case CheckTypeEstr.Range:
                            typeName = "Range";
                            var values = item.CheckData.Split(',');
                            var value2 = Convert.ToDecimal(value);
                            if (value2 < Convert.ToDecimal(values[0]) || value2 > Convert.ToDecimal(values[1]))
                                return "range not match: " + value + ", " + values[0] + ", " + values[1];
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                return "CrudEdit.cs ValidRow() failed: CheckType=" + typeName + ", msg=" + ex.Message;
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
            return "";
            #endregion
        }

        //is transaction or not
        private bool IsTrans(EditDto edit)
        {
            var childLen = GetEditChildLen(edit);
            return (_editDto.Transaction != null)
                ? _editDto.Transaction.Value
                : (childLen > 0);
        }

        /// <summary>
        /// save new rows, use transaction
        /// </summary>
        /// <param name="json"></param>
        /// <param name="fnAfterSaveA"></param>
        /// <returns>ResultDto</returns>
        public async Task<ResultDto> CreateA(JObject json,
            FnSetNewKeyJsonA? fnSetNewKeyA = null, FnAfterSaveA? fnAfterSaveA = null)
        {
            return await SaveJsonA(json, fnSetNewKeyA, fnAfterSaveA);
        }

        /// <summary>
        /// save updated rows(including delete rows), use transaction
        /// </summary>
        /// <param name="key">key of master table</param>
        /// <param name="json"></param>
        /// <param name="fnAfterSaveA"></param>
        /// <returns>ResultDto</returns>
        public async Task<ResultDto> UpdateA(string key, JObject json,
            FnSetNewKeyJsonA? fnSetNewKeyA = null, FnAfterSaveA? fnAfterSaveA = null)
        {
            //return error if empty key
            if (key == "")
                return _Model.GetError("CrudEdit.cs UpdateA() failed: key is empty.");

            //set instance variables
            _key = key;
            _isNew = false;

            //check for AuthType=Row if need
            if (_Fun.IsAuthTypeRow())
            {
                var data = await GetDbRowA(_editDto, key);    //return data
                var brError = CheckAuthRow(data!, CrudEnum.Update);
                if (brError != "") return _Model.GetBrError(brError);
            }

            /*
            //add kid value into json if empty
            var rows = json[_Fun.FidRows] as JArray;
            if (rows.Count == 1 && rows[0][_editDto.PkeyFid] == null)
                rows[0][_editDto.PkeyFid] = key;
            */

            return await SaveJsonA(json, fnSetNewKeyA, fnAfterSaveA);
        }

        /// <summary>
        /// save rows including delete rows, use transaction
        /// called by Create(), Update()
        /// </summary>
        /// <param name="inputJson">input json</param>
        /// <param name="fnSetNewKeyA">custom function for set newKeyJson</param>
        /// <param name="fnAfterSaveA"></param>
        /// <returns></returns>
        private async Task<ResultDto> SaveJsonA(JObject inputJson, 
            FnSetNewKeyJsonA? fnSetNewKeyA = null, FnAfterSaveA? fnAfterSaveA = null)
        {
            //check input & set fidNos same time
            var log = false;
            Db? db = null;
            string error;
            var trans = IsTrans(_editDto);
            if (inputJson == null)
            {
                log = true;
                error = "input json is null";
                goto lab_error;
            }

            //check main row
            //SetFidNo(_edit);
            error = ValidJsonLoop(0, _editDto, inputJson);
            if (_Str.NotEmpty(error))
            {
                log = true;
                goto lab_error;
            }

            //set new key if need
            //addFunName = false;
            if (_editDto.AutoIdLen > 0)
            {
                error = (fnSetNewKeyA == null)
                    ? await SetNewKeyJsonA(inputJson, _editDto)
                    : await fnSetNewKeyA(this, inputJson, _editDto);
                if (_Str.NotEmpty(error)) goto lab_error;
            }

            //transaction if need
            db = GetDb();
            if (trans) await db.BeginTranA();

            //set current time(_now)
            SetNow();

            //save db recursive
            if (!await SaveJsonLoopA("0", null, inputJson, _editDto, db)) 
                goto lab_error;

            //call AfterSave() if need
            if (fnAfterSaveA != null)
            {
                try
                {
                    error = await fnAfterSaveA(this, db, _newKeyJson);
                    if (_Str.NotEmpty(error)) goto lab_error;
                }
                catch (Exception ex)
                {
                    log = true;
                    error = "AfterSave error: " + ex.Message;
                    goto lab_error;
                }
            }

            //case of ok
            if (trans) await db.CommitA();
            await db.DisposeAsync();
            return new ResultDto() { Value = _saveRows.ToString() };

        lab_error:
            if (db != null)
            {
                if (trans) await db.RollbackA();
                await db.DisposeAsync();
            }

            if (log) await _Log.ErrorRootA("CrudEdit.cs SaveJsonA() failed: " + error);
            return _Model.GetError();
        }

        /// <summary>
        /// validate and save(recursive)
        /// </summary>
        /// <param name="levelStr">level concat string, ex:0,00,012</param>
        /// <param name="upDeletes">null for level0</param>
        /// <param name="inputJson">JObject for level0, JArray for level1/2</param>
        /// <param name="edit"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private async Task<bool> SaveJsonLoopA(string levelStr, List<string>? upDeletes, 
            JObject inputJson, EditDto edit, Db db)
        {
            //var hasInput = (inputJson != null);
            //if (inputJson == null)
            //    return true;

            var levelLen = levelStr.Length;
            var isLevel0 = (levelLen == 1);

            #region delete first & get deleted list for child(if need)
            List<string>? deletes = (inputJson[Deletes] == null)
                ? null : _Str.ToList(inputJson[Deletes]!.ToString());
            if (upDeletes != null)
                deletes = _List.Concat(deletes!, await GetKeysByUpKeysA(edit, upDeletes!, db));

            if (deletes != null)
            {
                //deleted key, no special char !!
                if (isLevel0 && !_List.CheckKey(deletes)) return false;

                //if no Fkey, use deleted key for child's upKey
                //if (!hasFkey)
                    //deletes = _List.Concat(deletes, await GetKeysByUpKeysAsync(edit, upDeletes, db));

                if (!await DeleteRowsByKeysA(edit, deletes, db)) return false;
            }
            #endregion

            //if (!hasInput) return true;

            #region insert/update this rows
            var inputRows = (inputJson[_Fun.FidRows] == null)
                ? null : inputJson[_Fun.FidRows] as JArray;
            //JObject upNewKey2 = new(); //new pkey for childs fkey
            if (inputRows != null)
            {
                //使用JToken再轉型JObject則不會出現null error的情形 !!
                var kid = edit.PkeyFid;
                foreach (JToken token in inputRows)
                {
                    //inputRow0 could be null, save to var first, or will error
                    //if (token == null) continue;

                    var inputRow = token as JObject;
                    if (_Json.IsEmpty(inputRow)) continue;

                    //insert/update this
                    //var inputRow = inputRow0 as JObject;
                    if (!HasInputField(inputRow, kid)) continue;

                    //注意: 移除if括號時無法執行else區段!!
                    if (IsNewRow(inputRow!))
                    {
                        if (!await InsertRowA(edit, inputRow!, db))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!await UpdateRowA(isLevel0, edit, inputRow!, db))
                        {
                            return false;
                        }
                    }
                }//for rows
            }//if
            #endregion

            #region insert/update childs(recursive)
            var childLen = GetEditChildLen(edit);
            for (var i = 0; i < childLen; i++)
            {
                var childJson = GetChildJson(inputJson, i);
                if (childJson == null) continue;

                //recursive call
                if (!await SaveJsonLoopA(levelStr + i, deletes, childJson, edit.Childs![i], db))
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
        /// set new key & _newKeyJson variables
        /// </summary>
        /// <param name="inputJson"></param>
        /// <param name="edit"></param>
        /// <returns>return error msg if any</returns>
        public async Task<string> SetNewKeyJsonA(JObject inputJson, EditDto edit)
        {
            return await SetNewKeyJsonLoopA("0", null, inputJson, edit);
        }

        /// <summary>
        /// (recursive)set new pkey & fkey (寫入inputJson), called by SetNewKeyJson()
        /// 前端會將所有空白pkey填入數字(上層資料序號, base 1)
        /// </summary>
        /// <param name="levelStr">level concat string, ex:0,00,012</param>
        /// <param name="upNewKey">null for level0, JObject for level1 after(f+idx(base 1)=value)
        /// <param name="inputJson">不可null, JObject 包含rows、childs欄位</param>
        /// <param name="edit"></param>
        /// <returns>error msg if any</returns>
        private async Task<string> SetNewKeyJsonLoopA(string levelStr, JObject? upNewKey, JObject inputJson, EditDto edit)
        {
            //if (inputJson == null) return "";

            var levelLen = levelStr.Length;
            var isLevel0 = (levelLen == 1);
            var isLevel1 = (levelLen == 2);

            #region insert/update this rows
            var error = "";
            var inputRows = (inputJson[_Fun.FidRows] == null)
                ? null : inputJson[_Fun.FidRows] as JArray;
            var setNewKey = false;  //flag
            JObject myNewKey = new(); //new pkey for childs fkey
            if (inputRows != null)
            {
                var kid = edit.PkeyFid;
                foreach (JToken token in inputRows)
                {
                    //inputRow0 could be null, save to var first, or will error
                    var inputRow0 = token as JObject;
                    if (_Json.IsEmpty(inputRow0)) continue;

                    //insert/update this
                    var inputRow = inputRow0 as JObject;
                    if (HasInputField(inputRow, kid))
                    {
                        //adjust pkeyIdx if need
                        var pkeyIdx = GetKeyIndex(inputRow!, kid);   //傳回0表示有key值
                        if (pkeyIdx < 0)
                        {
                            //case of 前端沒有傳入key欄位
                            //< 0 means empty pkey, main edit allows empty pkey
                            //if (isLevel0 && inputRows.Count == 1)

                            if ((_isNew && isLevel0) || isLevel1)
                                pkeyIdx = 1;    //表示新增

                            /*
                            if (isLevel0)
                            {
                                key = _isNew ? _Str.NewId(edit.AutoIdLen) : _key;
                                //if () pkeyIdx = 1;    //adjust, let it be new
                            }
                            else if (isLevel1)
                            {
                                key = _Str.NewId(edit.AutoIdLen);
                                //pkeyIdx = 1;    //表示新增
                            }
                            else
                            {
                                error = $"no PkeyFid ({edit.PkeyFid})";
                                goto lab_error;
                            }
                            */
                        }

                        //case of insert row
                        if (pkeyIdx > 0)
                        {
                            #region set foreign key value for not level0
                            if (!isLevel0)
                            {
                                var fkeyIdx = GetFkeyIndex(inputRow!);   //from 系統欄位(_fkeyfid)
                                if (fkeyIdx < 0)
                                {
                                    if (!isLevel1)
                                    {
                                        error = $"no FkeyFid ({edit.FkeyFid})";
                                        goto lab_error;
                                    }

                                    //case isLevel1, adjust, 由系統設定fkey(參考上一層, //todo: level0為多筆會出錯 !!)
                                    fkeyIdx = 1;    
                                }

                                //case of fkeyIdx >= 0
                                if (fkeyIdx == 0)
                                    inputRow![edit.FkeyFid] = inputRow[FkeyFid]!.ToString();  //是否必要??
                                else if (upNewKey == null)
                                {
                                    error = $"can not get upNewKey by FkeyFid ({edit.FkeyFid})";
                                    goto lab_error;
                                }
                                else
                                {
                                    //set fkey
                                    inputRow![edit.FkeyFid] = upNewKey["f" + fkeyIdx];
                                }
                            }
                            #endregion

                            //set new key if need & set newKeyJson
                            var key = (edit.FnGetNewKeyA == null)
                                ? _Str.NewId(edit.AutoIdLen)
                                : await edit.FnGetNewKeyA();
                            inputRow![IsNew] = "1";  //string
                            inputRow![kid] = key;

                            //set newKeyJson for child, pkeyIdx is base 1 
                            myNewKey["f" + pkeyIdx] = key;
                            setNewKey = true;
                        }
                    }//if row has fields
                }//for rows
            }//if has rows
            #endregion

            if (!setNewKey && isLevel0 && !_isNew)
                myNewKey["f1"] = _key;

            #region set childs new key (recursive)
            var childLen = GetEditChildLen(edit);
            for (var i = 0; i < childLen; i++)
            {
                var childJson = GetChildJson(inputJson, i);
                if (childJson == null) continue;

                //recursive call
                error = await SetNewKeyJsonLoopA(levelStr + i, myNewKey, childJson, edit.Childs![i]);
                if (_Str.NotEmpty(error)) return error;
            }
            #endregion

            //set instance variables & return
            _newKeyJson["t" + levelStr] = myNewKey;
            return "";

        lab_error:
            return "CrudEdit.cs SetNewKeyJson2() failed: " + error;
        }

        /// <summary>
        /// set child foreign key value
        /// called by FnSetNewKey()
        /// </summary>
        /// <param name="levelStr"></param>
        /// <returns>error msg if any</returns>
        public string SetChildFkey(JObject inputJson, int childIdx, string fid, string fromLevelStr)
        {
            //get child rows
            var rows = GetChildRows(inputJson, childIdx);
            if (rows == null) return "";

            //get table json first by levelStr
            string error;
            var jsonFid = "t" + fromLevelStr;
            var json = (_newKeyJson[jsonFid] == null)
                ? null : (JObject)_newKeyJson[jsonFid]!;

            foreach (JToken token in rows)
            {
                var row = token as JObject;
                if (_Json.IsEmpty(row)) continue;

                //has row need set or not
                //var row = row0 as JObject;
                var keyIndex = GetKeyIndex(row!, fid);
                if (keyIndex > 0)
                {
                    if (json == null)
                    {
                        error = $"no _newKeyJson[{jsonFid}]";
                        goto labError;
                    }

                    row![fid] = json["f" + keyIndex]!.ToString();
                }
            }

            //case ok ok
            return "";

        labError:
            return "CrudEdit.cs SetRelatId() failed: " + error;
        }

        /// <summary>
        /// delete row for single table
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<ResultDto> DeleteA(string key)
        {
            //check for AuthType=Row if need
            if (_Fun.IsAuthTypeRow())
            {
                var data = await GetDbRowA(_editDto, key);    //return data
                var brError = CheckAuthRow(data!, CrudEnum.Delete);
                if (_Str.NotEmpty(brError))
                    return _Model.GetBrError(brError);
            }

            return await DeleteByKeysA(new List<string>() { key });
        }

        /// <summary>
        /// delete rows of table
        /// </summary>
        /// <param name="keys">row key list</param>
        /// <returns></returns>
        public async Task<ResultDto> DeleteByKeysA(List<string> keys)
        {
            //check input
            if (!_List.CheckKey(keys)) return _Model.GetError();

            //transaction or not
            var trans = IsTrans(_editDto);
            var db = GetDb();
            if (trans) await db.BeginTranA();

            //set current time(_now)
            SetNow();

            var json = new JObject() { [Deletes] = _List.ToStr(keys) };
            if (!await SaveJsonLoopA("0", null, json, _editDto, db))
                goto lab_error;

            if (trans) await db.CommitA();
            await db.DisposeAsync();
            return new ResultDto();

        lab_error:
            if (trans) await db.RollbackA();
            await db.DisposeAsync();
            //TODO
            return _Model.GetError();
        }

        /// <summary>
        /// delete rows of one table, input pkey list
        /// </summary>
        /// <param name="edit"></param>
        /// <param name="keys">can be multi pkey value(consider seperator)</param>
        /// <param name="db"></param>
        /// <returns>error msg if any</returns>
        private async Task<bool> DeleteRowsByKeysA(EditDto edit, List<string> keys, Db? db = null)
        {
            //check input
            if (keys.Count == 0) return true;

            //reset
            ResetArg();

            //delete rows
            var newDb = _Db.CheckOpenDb(ref db);
            var values = "";
            //=== case of single pkey ===
            //set sql args
            var kid = edit.PkeyFid;
            for (var i = 0; i < keys.Count; i++)
            {
                var fid = edit.PkeyFid + i;
                AddArg(fid, keys[i].ToString());
                values += "@" + fid + ",";
            }

            //update db
            var sql = string.Format(_Fun.DeleteRowsSql, edit.Table, kid, values[0..^1]);
            var count = await db!.ExecSqlA(sql, _sqlArgs!);
            //if (count == 0)
            //    goto lab_error;
            await _Db.CheckCloseDbA(db, newDb);

            //case of ok
            _saveRows += count;
            return true;
        }

        private async Task<List<string?>?> GetKeysByUpKeysA(EditDto edit, List<string> upKeys, Db db)
        {
            if (upKeys.Count == 0) return null;

            var sql = string.Format("select {0} from {1} where {2} in ({3})", edit.PkeyFid, edit.Table, edit.FkeyFid, _List.ToStr(upKeys, true));
            return await db.GetStrsA(sql);
        }

    }//class
}
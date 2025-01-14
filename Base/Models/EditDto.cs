using Base.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Base.Models
{
    /// <summary>
    /// for crud edit form
    /// </summary>
    public class EditDto
    {
        /// <summary>
        /// (1st)transaction or not, for master table only
        /// null: 由系統自動控制
        /// </summary>
        public bool? Transaction = null;

        /// <summary>
        /// (1st)has foreign or not, 如果為true, 則由DB自行刪除關聯資料
        /// </summary>
        //public bool HasFKey = true;

        /// <summary>
        /// auto Id length, empty for default.<br/>
        /// values: _Fun.AutoIdShort(6)、_Fun.AutoIdMid(10)、_Fun.AutoIdLong(16)
        /// </summary>
        public int AutoIdLen = _Fun.AutoIdMid;

        /// <summary>
        /// table name
        /// </summary>
        public string Table = "";

        /// <summary>
        /// sql string, if not empty, will read db
        /// master/child-1 readSql must use "=", ex: Id=@Id
        /// child-2 readSql must use "in", ex: Id in ({0}), or Id in ({{0}})(has @ sign)
        /// </summary>
        public string ReadSql = "";

        /// <summary>
        /// primary key field id
        /// note: single pkey can not edit
        /// </summary>
        public string PkeyFid = "";

        /// <summary>
        /// (2nd)foreign key field id
        /// 這個欄位不可設為 required !!
        /// </summary>
        public string FkeyFid = "";

        /// <summary>
        /// field list
        /// </summary>
        public EitemDto[] Items = null!;

        /// <summary>
        /// field list for empty to null, usually for data field
        /// 空的日期欄位存入DB會變成1900/1/1, 必須先轉成null !!
        /// </summary>
        public string[] EmptyToNulls = [];

        /// <summary>
        /// Col4可為null, 元素也可為null
        /// creator, created datetime, reviser, revised datetime
        /// consider time difference
        /// default has 4 fields, set this to null if none
        /// </summary>
        public string?[]? Col4 = ["Creator", "Created", "Reviser", "Revised"];

        /// <summary>
        /// (2nd)order by string (not include "order by")
        /// </summary>
        public string OrderBy = "";

        /// <summary>
        /// child edit form list
        /// </summary>
        //public List<EditDto> Childs = null;
        public EditDto[]? Childs = null;

        /// <summary>
        /// fid no 對應, 儲存資料時會系統重新設定, PG不必處理
        /// 如果無異動欄位, 則此變數為 null
        /// </summary>
        public JObject? _FidNo = null;

        /// <summary>
        /// 必填欄位清單, 儲存資料時會系統重新設定, PG不必處理
        /// </summary>
        public List<string>? _FidRequires = null;

        /// <summary>
        /// 後端自定義欄位驗證
        /// <param name="bool">isNew fun or not</param>
        /// <returns>validate error list if any</returns>
        /// </summary>
        //public Func<bool, JObject, List<ErrorRowDto>?>? FnValidate = null;
        public FnValidate? FnValidate = null;

        /// <summary>
        /// crud edit AfterSave, inside transaction
        /// 參考 HrAdm LeaveEdit.cs CreateA()、BaoAdm BaoEdit.cs
        /// </summary>
        /// <param name="bool">isNew fun or not</param>
        /// <param name="CrudEditSvc"></param>
        /// <param name="Db"></param>
        /// <param name="JObject">keyJson</param>
        /// <returns>error msg if any</returns>
        public FnAfterSaveA? FnAfterSaveA = null;

        /// <summary>
        /// 自行函數 for 設定 new key
        /// </summary>
        public FnGetNewKeyA? FnGetNewKeyA = null;

        /// <summary>
        /// set new keyJson
        /// </summary>
        /// <param name="isNew">isNew fun or not</param>
        /// <param name="crudEditSvc">CrudEdit service</param>
        /// <param name="inputJson">input json</param>
        /// <param name="editDto">edit dto</param>
        /// <returns>error msg if any</returns>
        public FnSetNewKeyJsonA? FnSetNewKeyJsonA = null;
        //(bool isNew, CrudEditSvc crudEditSvc, JObject inputJson, EditDto editDto);

    }//class
}

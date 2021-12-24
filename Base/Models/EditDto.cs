using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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
        /// auto key value when create(call _Str.NewId())
        /// if false, then set by coding !!
        /// </summary>
        public bool AutoNewId = true;

        /// <summary>
        /// table name
        /// </summary>
        public string Table;

        /// <summary>
        /// sql string, if not empty, will read db
        /// master readSql must use "=", ex: Id='{0}', or Id='{{0}}'(has @ sign)
        /// child readSql must use "in", ex: Id in ({0}), or Id in ({{0}})(has @ sign)
        /// </summary>
        public string ReadSql = "";

        /// <summary>
        /// primary key field id
        /// note: single pkey can not edit
        /// </summary>
        public string PkeyFid;

        /// <summary>
        /// (2nd)foreign key field id
        /// 這個欄位不可設為 required !!
        /// </summary>
        public string FkeyFid;

        /// <summary>
        /// field list
        /// </summary>
        public EitemDto[] Items;

        /// <summary>
        /// field list for empty to null, usually for data field
        /// 空的日期欄位存入DB會變成1900/1/1, 必須先轉成null !!
        /// </summary>
        public string[] EmptyToNulls = new string[] { };

        /// <summary>
        /// creator, created datetime, reviser, revised datetime
        /// consider time difference
        /// default has 4 fields, set this to null if none
        /// </summary>
        public string[] Col4 = new string[] { "Creator", "Created", "Reviser", "Revised" };

        /// <summary>
        /// (2nd)order by string (not include "order by")
        /// </summary>
        public string OrderBy = "";

        /// <summary>
        /// child edit form list
        /// </summary>
        //public List<EditDto> Childs = null;
        public EditDto[] Childs = null;

        /// <summary>
        /// fid no 對應, 儲存資料時會系統重新設定, PG不必處理
        /// 如果無異動欄位, 則此變數為 null
        /// </summary>
        public JObject _FidNo = null;

        /// <summary>
        /// 必填欄位清單, 儲存資料時會系統重新設定, PG不必處理
        /// </summary>
        public List<string> _FidRequires = null;

    }//class
}

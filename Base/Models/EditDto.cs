using Base.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// crud edit form model
    /// </summary>
    public class EditDto
    {
        /// <summary>
        /// (用於Master table) 是否由系統自動設定 transaction, 預設為true, 如果要自行控制, 則設為false
        /// </summary>
        public bool AutoTrans = true;

        /// <summary>
        /// 自動產生主key的字串長度, 預設為中等長度 10 個字元, 全部清單有: 6、10、16<br/>
        /// 如果設為0則表示允許前端傳入新Id
        /// </summary>
        public int AutoIdLen = _Fun.AutoIdMid;

        /// <summary>
        /// 要存取的 table name
        /// </summary>
        public string Table = "";

        /// <summary>
        /// 讀取資料庫的 sql, 如果空白則表示使用 Items 欄位來組成 sql<br/>
        /// master table 和第一層 child table 此欄位最後面要加上 Id=@Id 來讀取該筆/多筆資料<br/>
        /// 第二層 child table 必須使用 in , ex: Id in ({0}), or Id in ({{0}})(有 $ 符號時), 若是上層是單筆也可用={0}較簡單<br/>
        /// 如果只填ReadSql 沒有 Items, 則需要同時設定 PkeyFid 欄位
        /// </summary>
        public string ReadSql = "";

        /// <summary>
        /// primary key field id
        /// </summary>
        public string PkeyFid = "";

        /// <summary>
        /// (用於 child table) foreign key field id, 空白表示不必設定, 一般用在child是1對1, 其主key與master相同 !!
        /// 這個欄位不可設為 required !!
        /// </summary>
        public string FkeyFid = "";

        /// <summary>
        /// 要存取的欄位清單
        /// </summary>
        public EitemDto[] Items = null!;

        /// <summary>
        /// 有些欄位如果傳入空白, 則要先轉成 NULL 再存入資料庫，則將欄位Id填入此欄位
        /// 例如: 空的日期欄位存入DB會變成1900/1/1, 必須先轉成null !!
        /// </summary>
        public string[] EmptyToNulls = [];

        /// <summary>
        /// 表示4個常見的欄位，依序為: Creator、Created、Reviser、Revised
        /// 預設有這4個欄位, 如果無則設為null
        /// 時間欄位系統會考慮登入者的時差問題
        /// </summary>
        public string?[]? Col4 = ["Creator", "Created", "Reviser", "Revised"];

        /// <summary>
        /// (用於 child table) 資料的排序, 此欄位本身不含 "order by" 關鍵字
        /// </summary>
        public string OrderBy = "";

        /// <summary>
        /// child table list
        /// </summary>
        public EditDto[]? Childs = null;

        /// <summary>
        /// 異動的 Fid 和序號的對照表, 儲存資料時會系統重新設定, 由系統自動設定
        /// 如果無異動欄位, 則此變數為 null
        /// </summary>
        public JObject? _FidNo = null;

        /// <summary>
        /// 必填欄位清單, 儲存資料時會系統重新設定, 由系統自動設定
        /// </summary>
        public List<string>? _FidRequires = null;

        /// <summary>
        /// 後端自定義欄位驗證檢查函數
        /// <param name="bool">isNew fun or not</param>
        /// <returns>驗證錯誤的欄位清單, 資料型態為 List<ErrorRowDto></returns>
        /// </summary>
        public FnValidateA? FnValidateA = null;

        /// <summary>
        /// 儲存資料之前要觸發的函數，發生在 transaction 之前
        /// 使用範例參考 DbAdm GenCrudUiEdit.cs
        /// </summary>
        /// <param name="bool">isNew fun or not</param>
        /// <param name="CrudEditSvc"></param>
        /// <param name="JObject">inputJson</param>
        /// <param name="JObject">newKeyJson</param>
        /// <returns>error msg if any</returns>
        public FnWhenSaveA? FnWhenSaveA = null;

        /// <summary>
        /// 儲存資料之後要觸發的函數，此時已經啟動 transaction, 並且完成儲存全部table, 但未commit
        /// 使用範例參考 HrAdm LeaveEdit.cs CreateA()、BaoAdm BaoEdit.cs
        /// </summary>
        /// <param name="bool">true(新增), false(修改)</param>
        /// <param name="CrudEditSvc"></param>
        /// <param name="Db"></param>
        /// <param name="JObject">keyJson</param>
        /// <returns>error msg if any</returns>
        public FnAfterSaveA? FnAfterSaveA = null;

        /// <summary>
        /// 自行函數用來傳回 new key
        /// </summary>
        public FnGetNewKeyA? FnGetNewKeyA = null;

        /// <summary>
        /// set new keyJson
        /// </summary>
        /// <param name="isNew">bool, isNew fun or not</param>
        /// <param name="crudEditSvc">CrudEdit service</param>
        /// <param name="inputJson">input json</param>
        /// <param name="editDto">EditDto</param>
        /// <returns>error msg if any</returns>
        public FnSetNewKeyJsonA? FnSetNewKeyJsonA = null;

    }//class
}

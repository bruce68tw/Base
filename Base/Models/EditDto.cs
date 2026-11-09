using Base.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// CRUD 主表或子表的讀寫設定，以及儲存流程的回呼與暫存狀態。
    /// </summary>
    public class EditDto
    {
        /// <summary>
        /// 允許自動交易，預設 true；目前僅有子表時才啟動自動交易。
        /// </summary>
        public bool AutoTrans = true;
        /// <summary>
        /// 自動主鍵字串長度，預設 _Fun.AutoIdMid；0 表示允許傳入新 Id。
        /// </summary>
        public int AutoIdLen = _Fun.AutoIdMid;
        /// <summary>
        /// 實際讀寫的資料表名稱，可含 schema。
        /// </summary>
        public string Table = "";
        /// <summary>
        /// 選填的單筆讀取 SQL；需 Join 其他資料表時設定，無 Join 且未啟用資料權限的單表讀取可省略，由系統依 Items 組出欄位清單。
        /// 啟用操作者資料權限時必須自訂 SQL 並關聯 XpUser，以 _Fun.FidUser、_Fun.FidDept 作為使用者 Id 與部門 Id 的回傳欄位別名。
        /// 主表與第一層子表使用 @Id，更深層子表以 {0} 帶入鍵清單。
        /// </summary>
        public string ReadSql = "";
        /// <summary>
        /// 主鍵欄位名稱。
        /// </summary>
        public string PkeyFid = "";
        /// <summary>
        /// 子表對應上層的外鍵欄位；可空白，不應設為必填欄位。
        /// </summary>
        public string FkeyFid = "";
        /// <summary>
        /// 可存取欄位設定；預設未初始化。
        /// </summary>
        public EitemDto[] Items = null!;
        /// <summary>
        /// 儲存時將空字串轉成資料庫 NULL 的欄位清單。
        /// </summary>
        public string[] EmptyToNulls = [];
        /// <summary>
        /// 依序為建立者、建立時間、修改者、修改時間欄位；表中沒有時可設 null。
        /// </summary>
        public string?[]? Col4 = ["Creator", "Created", "Reviser", "Revised"];
        /// <summary>
        /// 子表排序片段，不含 ORDER BY 關鍵字。
        /// </summary>
        public string OrderBy = "";
        /// <summary>
        /// 下一層子表設定，無子表時為 null。
        /// </summary>
        public EditDto[]? Childs = null;
        /// <summary>
        /// 儲存流程建立的欄位名稱與 Items 索引對照。
        /// </summary>
        public JObject? _FidNo = null;
        /// <summary>
        /// 儲存流程建立的必填欄位清單。
        /// </summary>
        public List<string>? _FidRequires = null;
        /// <summary>
        /// 非同步自訂資料驗證，回傳欄位錯誤清單。
        /// </summary>
        public FnValidateA? FnValidateA = null;
        /// <summary>
        /// 儲存前、交易啟動前的非同步回呼，回傳錯誤字串。
        /// </summary>
        public FnWhenSaveA? FnWhenSaveA = null;
        /// <summary>
        /// 資料儲存後、可能的交易提交前回呼，回傳錯誤字串。
        /// </summary>
        public FnAfterSaveA? FnAfterSaveA = null;
        /// <summary>
        /// 非同步產生新主鍵的自訂回呼。
        /// </summary>
        public FnGetNewKeyA? FnGetNewKeyA = null;
        /// <summary>
        /// 自訂設定新主鍵 JSON 的非同步回呼，回傳錯誤字串。
        /// </summary>
        public FnSetNewKeyJsonA? FnSetNewKeyJsonA = null;

    }//class
}

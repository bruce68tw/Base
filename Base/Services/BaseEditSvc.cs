using Base.Enums;
using Base.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// 以 GetDto 提供設定，延遲建立讀取與異動服務的 CRUD 包裝基底。
    /// </summary>
    abstract public class BaseEditSvc
    {
        /// <summary>
        /// 作業名稱，供草稿與權限處理使用。
        /// </summary>
        public string Ctrl;

        //public CrudEnum Fun;

        /// <summary>
        /// 修改資料時是否優先讀取草稿。
        /// </summary>
        public bool HasDraft;

        private CrudGetSvc _getSvc = null!;
        private CrudEditSvc _editSvc = null!;

        //private EditDto _editDto = null!;

        //public FnValidate? fnValidate { get; set; }
        //public Func<JObject, List<ErrorRowDto>?>? FnValidate { get; set; }

        /// <summary>
        /// 設定作業名稱與草稿選項。
        /// </summary>
        public BaseEditSvc(string ctrl, bool hasDraft = false) 
        {
            Ctrl = ctrl;
            HasDraft = hasDraft;
        }

        /// <summary>
        /// 由子類別提供目前操作適用的主從表設定。
        /// </summary>
        abstract public EditDto GetDto();

        /// <summary>
        /// 取得延遲建立的異動服務，不與讀取服務自動共用 Db。
        /// </summary>
        public CrudEditSvc EditSvc()
        {
            //return new CrudEditSvc(Ctrl, GetDto());
            _editSvc ??= new CrudEditSvc(Ctrl);
            return _editSvc;
        }

        /// <summary>
        /// 取得延遲建立的讀取服務，建立時套用 HasDraft。
        /// </summary>
        public CrudGetSvc GetSvc()
        {
            //return new CrudGetSvc(Ctrl, GetDto(), HasDraft);
            _getSvc ??= new CrudGetSvc(Ctrl, HasDraft);
            return _getSvc;
        }

        /// <summary>
        /// 取得讀取服務的 Db，並非異動服務的交易連線。
        /// </summary>
        public Db GetDb(bool outside = false)
        {
            return GetSvc().GetDb(outside);
        }

        /// <summary>
        /// 讀取修改用主從資料，可能優先採用草稿。
        /// </summary>
        public virtual async Task<JObject?> GetUpdJsonA(string key)
        {
            //Fun = fun;
            //var fun = CrudEnum.Update;
            return await GetSvc().GetUpdJsonA(key, GetDto());
        }

        /// <summary>
        /// 讀取檢視用主從資料。
        /// </summary>
        public virtual async Task<JObject?> GetViewJsonA(string key)
        {
            //Fun = fun;
            //var fun = CrudEnum.View;
            return await GetSvc().GetViewJsonA(key, GetDto());
        }

        /// <summary>
        /// 委派讀取服務的簽核資料入口，使用新增功能模式。
        /// </summary>
        public virtual async Task<JObject?> GetSignJsonA(string key)
        {
            return await GetSvc().GetSignJsonA(key, GetDto());
        }

        /// <summary>
        /// 讀取目前使用者的草稿，無內容時可為 null。
        /// </summary>
        public virtual async Task<JObject?> GetDraftJsonA(string key)
        {
            return await GetSvc().GetDraftJsonA(key);
        }

        /// <summary>
        /// 依指定 CRUD 功能讀取主從資料。
        /// </summary>
        public virtual async Task<JObject?> GetJsonByFunA(string key, CrudEnum fun)
        {
            return await GetSvc().GetJsonByFunA(fun, key, GetDto());
        }

        /// <summary>
        /// 以 GetDto 的設定委派新增，輸入須符合 _rows 契約。
        /// </summary>
        public virtual async Task<ResultDto> CreateA(JObject json)
        {
            return await EditSvc().CreateA(json, GetDto());
        }

        /// <summary>
        /// 以主鍵及 GetDto 的設定委派修改。
        /// </summary>
        public virtual async Task<ResultDto> UpdateA(string key, JObject json)
        {
            return await EditSvc().UpdateA(key, json, GetDto());
        }

        /// <summary>
        /// 將輸入 JSON 委派寫入草稿檔。
        /// </summary>
        public virtual async Task<ResultDto> DraftA(string key, JObject json)
        {
            return await EditSvc().DraftA(key, json);
        }

        /// <summary>
        /// 以主鍵及 GetDto 的設定委派刪除。
        /// </summary>
        public virtual async Task<ResultDto> DeleteA(string key)
        {
            return await EditSvc().DeleteA(key, GetDto());
        }

    }//class
}

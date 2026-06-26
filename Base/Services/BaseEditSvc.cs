using Base.Enums;
using Base.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// auto Ctrl variables for all xxxEdit.cs/xxxE.cs
    /// 包含 CrudGet、CrudEdit 功能
    /// </summary>
    abstract public class BaseEditSvc
    {
        /// <summary>
        /// controller name, auto set
        /// </summary>
        public string Ctrl;

        /// <summary>
        /// 目前功能
        /// </summary>
        public CrudEnum Fun;

        /// <summary>
        /// 是否有草稿功能
        /// </summary>
        public bool HasDraft;

        private CrudGetSvc _getSvc = null!;
        private CrudEditSvc _editSvc = null!;

        //private EditDto _editDto = null!;

        /// <summary>
        /// 自行函數 for 設定 new key
        /// </summary>
        //public FnValidate? fnValidate { get; set; }
        //public Func<JObject, List<ErrorRowDto>?>? FnValidate { get; set; }

        public BaseEditSvc(string ctrl, bool hasDraft = false) 
        {
            Ctrl = ctrl;
            HasDraft = hasDraft;
        }

        //derived class implement.
        abstract public EditDto GetDto();

        //EditService -> EditSvc
        public CrudEditSvc EditSvc()
        {
            //return new CrudEditSvc(Ctrl, GetDto());
            _editSvc ??= new CrudEditSvc(Ctrl);
            return _editSvc;
        }

        //GetService -> GetSvc
        //GetSvc不會設定EditDto內容, 因為其內容可能與Fun有關, ex:Early EvalueEdit.cs
        public CrudGetSvc GetSvc()
        {
            //return new CrudGetSvc(Ctrl, GetDto(), HasDraft);
            _getSvc ??= new CrudGetSvc(Ctrl, HasDraft);
            return _getSvc;
        }

        public virtual async Task<JObject?> GetUpdJsonA(string key)
        {
            //Fun = fun;
            //var fun = CrudEnum.Update;
            return await GetSvc().GetUpdJsonA(key, GetDto());
        }

        public virtual async Task<JObject?> GetViewJsonA(string key)
        {
            //Fun = fun;
            //var fun = CrudEnum.View;
            return await GetSvc().GetViewJsonA(key, GetDto());
        }

        public virtual async Task<JObject?> GetSignJsonA(string key)
        {
            return await GetSvc().GetSignJsonA(key, GetDto());
        }

        public virtual async Task<JObject?> GetDraftJsonA(string key)
        {
            return await GetSvc().GetDraftJsonA(key);
        }

        public virtual async Task<JObject?> GetJsonByFunA(string key, CrudEnum fun)
        {
            return await GetSvc().GetJsonByFunA(fun, key, GetDto());
        }

        //save new
        public virtual async Task<ResultDto> CreateA(JObject json)
        {
            return await EditSvc().CreateA(json, GetDto());
        }

        //save updatge, can override
        public virtual async Task<ResultDto> UpdateA(string key, JObject json)
        {
            return await EditSvc().UpdateA(key, json, GetDto());
        }

        public virtual async Task<ResultDto> DraftA(string key, JObject json)
        {
            return await EditSvc().DraftA(key, json);
        }

        public virtual async Task<ResultDto> DeleteA(string key)
        {
            return await EditSvc().DeleteA(key, GetDto());
        }

    }//class
}

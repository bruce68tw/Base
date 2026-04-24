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
        /// 是否有草稿功能
        /// </summary>
        public bool HasDraft;

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
            return new CrudEditSvc(Ctrl, GetDto());
        }

        //GetService -> GetSvc
        public CrudGetSvc GetSvc()
        {
            return new CrudGetSvc(Ctrl, GetDto(), HasDraft);
        }

        public virtual async Task<JObject?> GetUpdJsonA(string key)
        {
            return await GetSvc().GetUpdJsonA(key);
        }

        public virtual async Task<JObject?> GetViewJsonA(string key)
        {
            return await GetSvc().GetViewJsonA(key);
        }

        public virtual async Task<JObject?> GetDraftJsonA(string key)
        {
            return await GetSvc().GetDraftJsonA(key);
        }

        //save new
        public virtual async Task<ResultDto> CreateA(JObject json)
        {
            return await EditSvc().CreateA(json);
        }

        //save updatge, can override
        public virtual async Task<ResultDto> UpdateA(string key, JObject json)
        {
            return await EditSvc().UpdateA(key, json);
        }

        public virtual async Task<ResultDto> DraftA(string key, JObject json)
        {
            return await EditSvc().DraftA(key, json);
        }

        public virtual async Task<ResultDto> DeleteA(string key)
        {
            return await EditSvc().DeleteA(key);
        }

    }//class
}

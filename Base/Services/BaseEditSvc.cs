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
        public string Ctrl;

        public BaseEditSvc(string ctrl) 
        {
            Ctrl = ctrl; 
        }

        //derived class implement.
        abstract public EditDto GetDto();

        public CrudEditSvc EditService()
        {
            return new CrudEditSvc(Ctrl, GetDto());
        }

        public CrudGetSvc GetService()
        {
            return new CrudGetSvc(Ctrl, GetDto());
        }

        public async Task<JObject?> GetUpdJsonA(string key)
        {
            return await GetService().GetUpdJsonA(key);
        }

        public async Task<JObject?> GetViewJsonA(string key)
        {
            return await GetService().GetViewJsonA(key);
        }

        public async Task<ResultDto> CreateA(JObject json)
        {
            return await EditService().CreateA(json);
        }

        //can override
        public virtual async Task<ResultDto> UpdateA(string key, JObject json)
        {
            return await EditService().UpdateA(key, json);
        }

        public async Task<ResultDto> DeleteA(string key)
        {
            return await EditService().DeleteA(key);
        }

    }//class
}

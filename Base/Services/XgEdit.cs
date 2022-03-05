using Base.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Base.Services
{
    //auto Ctrl variables for all xxxEdit.cs
    abstract public class XgEdit
    {
        public string Ctrl;

        public XgEdit(string ctrl) 
        {
            Ctrl = ctrl; 
        }

        //derived class implement.
        abstract public EditDto GetDto();

        public CrudEdit EditService()
        {
            return new CrudEdit(Ctrl, GetDto());
        }

        public CrudGet GetService()
        {
            return new CrudGet(Ctrl, GetDto());
        }

        public async Task<JObject> GetUpdJsonAsync(string key)
        {
            return await GetService().GetUpdJsonAsync(key);
        }

        public async Task<JObject> GetViewJsonAsync(string key)
        {
            return await GetService().GetViewJsonAsync(key);
        }

        public async Task<ResultDto> CreateAsync(JObject json)
        {
            return await EditService().CreateAsync(json);
        }

        //can override
        public virtual async Task<ResultDto> UpdateAsync(string key, JObject json)
        {
            return await EditService().UpdateAsync(key, json);
        }

        public async Task<ResultDto> DeleteAsync(string key)
        {
            return await EditService().DeleteAsync(key);
        }

    }//class
}

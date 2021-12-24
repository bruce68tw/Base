using Base.Enums;
using Base.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

/// <summary>
/// 1.remove cache function
/// 2.add read/write multiple table fun
/// </summary>
namespace Base.Services
{
    /// <summary>
    /// for Crud Edit Service
    /// </summary>
    public class CrudGet : CrudBase
    {
        //constructor
        public CrudGet(string ctrl, EditDto editDto, string dbStr = "")
            : base(ctrl, editDto, dbStr)
        {
            _ctrl = ctrl;
            _editDto = editDto;
            _dbStr = dbStr;
        }

        public async Task<JObject> GetUpdJsonAsync(string key)
        {
            return await GetJsonByFunAsync(CrudEnum.Update, key);
        }
        public async Task<JObject> GetViewJsonAsync(string key)
        {
            return await GetJsonByFunAsync(CrudEnum.View, key);
        }

    }//class
}
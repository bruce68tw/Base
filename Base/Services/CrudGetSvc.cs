using Base.Enums;
using Base.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

/// <summary>
/// 繼承 CrudEditGetSvc, 利用 EditDto 來讀取資料庫資料, 簡化子代內容
/// 1.remove cache function
/// 2.add read/write multiple table fun
/// </summary>
namespace Base.Services
{
    /// <summary>
    /// for Crud Edit Service
    /// </summary>
    public class CrudGetSvc : CrudEditGetSvc
    {
        //constructor
        public CrudGetSvc(string ctrl, EditDto editDto, string dbStr = "")
            : base(ctrl, editDto, dbStr)
        {
            _ctrl = ctrl;
            _editDto = editDto;
            _dbStr = dbStr;
        }

        public async Task<JObject?> GetUpdJsonA(string key)
        {
            return await GetJsonByFunA(CrudEnum.Update, key);
        }
        public async Task<JObject?> GetViewJsonA(string key)
        {
            return await GetJsonByFunA(CrudEnum.View, key);
        }

    }//class
}
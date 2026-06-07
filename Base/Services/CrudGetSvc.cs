using Base.Enums;
using Base.Models;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// 繼承 CrudEditGetSvc, 利用 EditDto 來讀取資料庫資料, 簡化子代內容
/// 1.remove cache function
/// 2.add read/write multiple table fun
/// </summary>
namespace Base.Services
{
    /// <summary>
    /// for Crud Edit Service 讀取一筆資料
    /// </summary>
    public class CrudGetSvc : CrudEditGetSvc
    {
        //是否有草稿功能，讀取資料時先找草稿，沒有再找正式資料
        protected bool _hasDraft = false;

        //constructor
        public CrudGetSvc(string ctrl, bool hasDraft = false, string dbStr = "")
            : base(ctrl, dbStr)
        {
            _ctrl = ctrl;
            //_editDto = editDto;
            _hasDraft = hasDraft;
            _dbStr = dbStr;
        }

        public async Task<JObject?> GetUpdJsonA(string key, EditDto editDto)
        {
            //檢查權限
            //Fun = fun;
            //_editDto = editDto;
            var json = _hasDraft ? await GetDraftJsonA(key) : null;
            return (json == null)
                ? await GetJsonByFunA(CrudEnum.Update, key, editDto)
                : json;
        }

        public async Task<JObject?> GetViewJsonA(string key, EditDto editDto)
        {
            //_editDto = editDto;
            return await GetJsonByFunA(CrudEnum.View, key, editDto);
        }

        public async Task<JObject?> GetSignJsonA(string key, EditDto editDto)
        {
            //_editDto = editDto;
            return await GetJsonByFunA(CrudEnum.Create, key, editDto);
        }

        public async Task<JObject?> GetDraftJsonA(string key)
        {
            //使用: 程式Id_userId_rowId 當 key 去找草稿資料(檔案)，沒有再找正式資料
            //key = _Str.EmptyToValue(key, "-1");     //-1表示新增
            //var path = $"{_Fun.DirDraft}{_ctrl}_{_Fun.UserId()}_{key}.json";
            var path = GetDraftPath(key);
            if (File.Exists(path))
            {
                var str = await _File.ToStrA(path);
                return string.IsNullOrEmpty(str)
                    ? null  : _Str.ToJson(str);
            }
            else
            {
                return null;
            }
        }

    }//class
}
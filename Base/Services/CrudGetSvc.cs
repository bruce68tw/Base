using Base.Enums;
using Base.Models;
using Newtonsoft.Json.Linq;
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
    /// 依 EditDto 讀取編輯、檢視及草稿資料。
    /// </summary>
    public class CrudGetSvc : CrudEditGetSvc
    {
        //是否有草稿功能，讀取資料時先找草稿，沒有再找正式資料
        protected bool _hasDraft = false;

        /// <summary>
        /// 設定作業名稱、草稿選項與連線字串。
        /// </summary>
        public CrudGetSvc(string ctrl, bool hasDraft = false, string dbStr = "")
            : base(ctrl, dbStr)
        {
            _ctrl = ctrl;
            _hasDraft = hasDraft;
            _dbStr = dbStr;
        }

        /// <summary>
        /// 啟用草稿時優先讀草稿，否則依修改功能讀取正式資料。
        /// </summary>
        public async Task<JObject?> GetUpdJsonA(string key, EditDto editDto)
        {
            //檢查權限
            var json = _hasDraft ? await GetDraftJsonA(key) : null;
            return (json == null)
                ? await GetJsonByFunA(CrudEnum.Update, key, editDto)
                : json;
        }

        /// <summary>
        /// 依檢視功能讀取主從資料及資料範圍結果。
        /// </summary>
        public async Task<JObject?> GetViewJsonA(string key, EditDto editDto)
        {
            return await GetJsonByFunA(CrudEnum.View, key, editDto);
        }

        /// <summary>
        /// 依新增功能讀取資料；基底讀取流程因此不套用既有列的範圍檢查。
        /// </summary>
        public async Task<JObject?> GetSignJsonA(string key, EditDto editDto)
        {
            return await GetJsonByFunA(CrudEnum.Create, key, editDto);
        }

        /// <summary>
        /// 讀取使用者草稿 JSON；不存在、內容為空或格式無效時回傳 null。
        /// </summary>
        public async Task<JObject?> GetDraftJsonA(string key)
        {
            //使用: 程式Id_userId_rowId 當 key 去找草稿資料(檔案)，沒有再找正式資料
            //key = _Str.EmptyToValue(key, "-1");     //-1表示新增
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

using Base.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

//使用 delegate 在實作時可以提供函數的語法提示, Func則沒有參數提示
//以屬性的方式設定
//屬性名稱和實作函數都使用相同名稱(不會有衝突)
namespace Base.Services
{
    //public delegate List<ErrorRowDto>? FnValidate(bool isNew, JObject inputJson);
    /// <summary>
    /// 非同步檢查輸入 JSON，isNew 表示新增；無欄位錯誤可回傳 null 或空清單。
    /// </summary>
    public delegate Task<List<ErrorRowDto>?> FnValidateA(bool isNew, JObject inputJson);

    /// <summary>
    /// 儲存前且交易啟動前回呼；isNew 表示新增，空字串代表無錯誤。
    /// </summary>
    public delegate Task<string> FnWhenSaveA(bool isNew, CrudEditSvc crudEditSvc, JObject inputJson, JObject keyJson);

    /// <summary>
    /// 儲存後、可能的交易提交前回呼；使用傳入 Db，空字串代表無錯誤。
    /// </summary>
    public delegate Task<string> FnAfterSaveA(bool isNew, CrudEditSvc crudEditSvc, Db db, JObject keyJson);

    /// <summary>
    /// 非同步產生新主鍵字串。
    /// </summary>
    public delegate Task<string> FnGetNewKeyA();

    /// <summary>
    /// 自訂主鍵與 JSON 設定流程，成功回傳空字串。
    /// </summary>
    public delegate Task<string> FnSetNewKeyJsonA(bool isNew, CrudEditSvc crudEditSvc, JObject inputJson, EditDto editDto);

    /// <summary>
    /// 檢查單筆匯入資料，回傳錯誤文字或空字串。
    /// </summary>
    public delegate string FnCheckImportRow<T>(T row) where T : class, new();

    /// <summary>
    /// 儲存通過檢查的匯入列，回傳各筆錯誤文字，空字串項目表示成功。
    /// </summary>
    public delegate List<string> FnSaveImportRows<T>(List<T> okRows) where T : class, new();

    /// <summary>
    /// 非同步儲存匯入列，回傳各筆錯誤文字，空字串項目表示成功。
    /// </summary>
    public delegate Task<List<string>> FnSaveImportRowsA<T>(List<T> okRows) where T : class, new();

    /// <summary>
    /// 簽核完成或退回後，以流程狀態與共用 Db 執行回呼，成功回傳空字串。
    /// </summary>
    public delegate Task<string> FnAfterSignRowA(string flowStatus, Db db);
}

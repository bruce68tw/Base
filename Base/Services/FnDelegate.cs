using Base.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

//使用 delegate 在實作時可以提供函數的語法提示, Func則沒有參數提示
//以屬性的方式設定
//屬性名稱和實作函數都使用相同名稱(不會有衝突)
namespace Base.Services
{
    /// <summary>
    /// 檢查輸入欄位, 無誤傳回null
    /// 參考 DbAdm IssueEdit.cs
    /// </summary>
    /// <param name="isNew">isNew fun or not</param>
    /// <param name="inputJson">input json</param>
    /// <returns>validate error list</returns>
    public delegate List<ErrorRowDto>? FnValidate(bool isNew, JObject inputJson);

    /// <summary>
    /// crud edit AfterSave, inside transaction
    /// 參考 HrAdm LeaveEdit.cs CreateA()、BaoAdm BaoEdit.cs
    /// </summary>
    /// <param name="isNew">isNew fun or not</param>
    /// <param name="crudEditSvc">CrudEdit service</param>
    /// <param name="db"></param>
    /// <param name="keyJson"></param>
    /// <returns>error msg if any</returns>
    public delegate Task<string> FnAfterSaveA(bool isNew, CrudEditSvc crudEditSvc, Db db, JObject keyJson);

    /// <summary>
    /// get new master key string
    /// </summary>
    /// <returns>new key string</returns>
    public delegate Task<string> FnGetNewKeyA();

    /// <summary>
    /// set new keyJson
    /// 參考 HrAdm XgFlowE.cs
    /// </summary>
    /// <param name="isNew">isNew fun or not</param>
    /// <param name="crudEditSvc">CrudEdit service</param>
    /// <param name="inputJson">input json</param>
    /// <param name="editDto">edit dto</param>
    /// <returns>error msg if any</returns>
    public delegate Task<string> FnSetNewKeyJsonA(bool isNew, CrudEditSvc crudEditSvc, JObject inputJson, EditDto editDto);

    /// <summary>
    /// check import row, return error msg if any
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="row"></param>
    /// <returns></returns>
    public delegate string FnCheckImportRow<T>(T row) where T : class, new();

    /// <summary>
    /// save import rows
    /// </summary>
    /// <param name="okRows"></param>
    /// <returns>list error string, '' for success</returns>
    public delegate List<string> FnSaveImportRows<T>(List<T> okRows) where T : class, new();

}

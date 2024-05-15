using Base.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Base.Services
{
    /// <summary>
    /// set new keyJson
    /// </summary>
    /// <param name="db"></param>
    /// <returns>error msg if any</returns>
    public delegate Task<string> FnSetNewKeyJsonA(CrudEditSvc editService, JObject inputJson, EditDto edit);

    /// <summary>
    /// crud edit AfterSave, inside transaction
    /// 參考 HrAdm LeaveEdit.cs CreateA()、BaoAdm BaoEdit.cs
    /// </summary>
    /// <param name="db"></param>
    /// <param name="keyJson"></param>
    /// <returns>error msg if any</returns>
    public delegate Task<string> FnAfterSaveA(CrudEditSvc editService, Db db, JObject keyJson);
    
    /// <summary>
    /// check row, return error msg if any
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

    /// <summary>
    /// save import rows
    /// </summary>
    /// <returns>list error string, '' for success</returns>
    public delegate Task<string> FnGetNewKeyA();

}

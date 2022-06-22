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
    public delegate string FnSetNewKeyJson(CrudEdit editService, JObject inputJson, EditDto edit);

    /// <summary>
    /// crud edit AfterSave
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public delegate Task<string> FnAfterSaveAsync(Db db, JObject keyJson);
    
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

}

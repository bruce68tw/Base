using Mongo.Models;
using Newtonsoft.Json.Linq;

//使用 delegate 在實作時可以提供函數的語法提示, Func則沒有參數提示
//以屬性的方式設定
//屬性名稱和實作函數都使用相同名稱(不會有衝突)
namespace Mongo.Services
{

    /// <summary>
    /// crud edit WhenSave before transaction
    /// 參考 DbAdm UiEdit.cs
    /// </summary>
    /// <param name="isNew">isNew fun or not</param>
    /// <param name="mgoEditSvc">CrudEdit service</param>
    /// <param name="inputJson">input json</param>
    /// <param name="keyJson"></param>
    /// <returns>error msg if any</returns>
    public delegate Task<string> FnMgoWhenSaveA(bool isNew, MgoEditSvc mgoEditSvc, JObject inputJson, JObject keyJson);

    /// <summary>
    /// crud edit AfterSave, inside transaction
    /// 參考 HrAdm LeaveEdit.cs CreateA()、BaoAdm BaoEdit.cs
    /// </summary>
    /// <param name="isNew">isNew fun or not</param>
    /// <param name="mgoEditSvc">CrudEdit service</param>
    /// <param name="db"></param>
    /// <param name="keyJson"></param>
    /// <returns>error msg if any</returns>
    public delegate Task<string> FnMgoAfterSaveA(bool isNew, MgoEditSvc mgoEditSvc, MgoDb db, JObject keyJson);

    /// <summary>
    /// set new keyJson
    /// 參考 HrAdm XgFlowE.cs
    /// </summary>
    /// <param name="isNew">isNew fun or not</param>
    /// <param name="mgoEditSvc">CrudEdit service</param>
    /// <param name="inputJson">input json</param>
    /// <param name="editDto">edit dto</param>
    /// <returns>error msg if any</returns>
    public delegate Task<string> FnMgoSetNewKeyJsonA(bool isNew, MgoEditSvc mgoEditSvc, JObject inputJson, MgoEditDto editDto);

    /// <summary>
    /// 簽核完成後的處理, 包含退回
    /// </summary>
    /// <param name="okRows"></param>
    /// <returns>error msg if any</returns>
    public delegate Task<string> FnMgoAfterSignRowA(string flowStatus, MgoDb db);
}

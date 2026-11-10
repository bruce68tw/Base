using Base.Enums;
using Base.Models;
using Base.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace BaseApi.Controllers
{
    /// <summary>
    /// 提供 MVC 控制器共用的 JSON、錯誤及檔案回應轉換方法。
    /// </summary>
    public class BaseCtrl : Controller 
    {
        //控制器名稱。
        /// <summary>
        /// 目前控制器的類別名稱，建構時移除名稱中的 Controller 字樣。
        /// </summary>
        public string Ctrl;

        //建構時由實際類別名稱取得控制器名稱。
        /// <summary>
        /// 依執行個體的類別名稱初始化控制器名稱。
        /// </summary>
        public BaseCtrl()
        {
            Ctrl = this.GetType().Name.Replace("Controller", "");
        }

        /*
        override public void OnActionExecuting(ActionExecutingContext context)
        {
            //put constructor will not work !!
            Ctrl = ControllerContext.ActionDescriptor.ControllerName;
            base.OnActionExecuting(context);
        }
        */

        /// <summary>
        /// 將錯誤訊息包裝成共用 JSON 錯誤格式，回傳 JSON ContentResult。
        /// </summary>
        /// <param name="error">要放進共用 JSON 錯誤物件的訊息。</param>
        [NonAction]
        public ContentResult CntError(string error)
        {
            return JsonToCnt(_Json.GetError(error));
        }

        /// <summary>
        /// 將既有字串原樣以 JSON 內容類型回傳。只有 null 會依 emptyToMsg 改用查無資料訊息或空字串。
        /// </summary>
        /// <param name="str">要原樣回傳的 JSON 字串。</param>
        /// <param name="emptyToMsg">輸入為 null 時是否回傳查無資料訊息。否則使用空字串。</param>
        [NonAction]
        public ContentResult JsonStrToCnt(string str, bool emptyToMsg = true)
        {
            str ??= emptyToMsg ? _Str.GetBrError("FindNone") : "";
            return Content(str, ContentTypeEstr.Json);
        }

        /// <summary>
        /// 將 JSON 物件包裝成 JSON ContentResult。
        /// 輸入為 null 時，依 emptyToMsg 改用查無資料的錯誤物件或空 JSON 物件。
        /// </summary>
        /// <param name="json">要序列化的 JSON 物件，可為 null。</param>
        /// <param name="emptyToMsg">輸入為 null 時是否回傳查無資料錯誤。否則使用空 JSON 物件。</param>
        [NonAction]
        public ContentResult JsonToCnt(JObject? json, bool emptyToMsg = true)
        {
            json ??= emptyToMsg ? _Json.GetBrError("FindNone") : [];
            return Content(json.ToString(), ContentTypeEstr.Json);
        }

        /// <summary>
        /// 將資料列轉成緊湊的 JSON 陣列。輸入為 null 時回傳空陣列。
        /// </summary>
        /// <param name="rows">要序列化的資料列陣列，null 視為空陣列。</param>
        [NonAction]
        public ContentResult JsonsToCnt(JArray? rows)
        {
            //如果傳回空字串前端會parser error !!
            string json = rows?.ToString(Newtonsoft.Json.Formatting.None) ?? "[]";
            return Content(json, ContentTypeEstr.Json);
        }

        /// <summary>
        /// 依 DTO 的串流、內容類型和檔名建立下載回應。有錯誤訊息時回傳 HTTP 400。
        /// </summary>
        /// <param name="file">包含檔案串流、內容類型、下載名稱或錯誤訊息的 DTO。</param>
        [NonAction]
        public IActionResult FileDtoToResult(DownFileDto file)
        {
            return (string.IsNullOrEmpty(file.Error))
                ? File(file.Stream!, file.ContentType, file.FileName)
                : BadRequest(file.Error);
        }

        //供衍生控制器回傳檔案結果。
        /// <summary>
        /// 回傳已建立的檔案結果。輸入為 null 時回傳 HTTP 404。
        /// </summary>
        /// <param name="file">要回傳的檔案結果，null 會轉成 HTTP 404。</param>
        [NonAction]
        public IActionResult FileToResult(FileResult? file)
        {
            return (file == null) ? NotFound() : file;
        }

    }//class
}

using Base.Enums;
using Base.Models;
using Base.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace BaseApi.Controllers
{
    public class BaseCtrl : Controller 
    {
        //controller name
        public string Ctrl;

        //new way.
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
        /// json to content result
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public ContentResult JsonToCnt(JObject? json)
        {
            json ??= _Json.GetBrError("FindNone");
            return Content(json.ToString(), ContentTypeEstr.Json);
        }

        public ContentResult JsonsToCnt(JArray? rows)
        {
            //如果傳回空字串前端會parser error !!
            string json = rows?.ToString(Newtonsoft.Json.Formatting.None) ?? "[]";
            return Content(json, ContentTypeEstr.Json);
        }

        public IActionResult FileDtoToResult(DownFileDto file)
        {
            return (string.IsNullOrEmpty(file.Error))
                ? File(file.Stream, file.ContentType, file.FileName)
                : BadRequest(file.Error);
        }

        public IActionResult ViewFile(FileResult? file)
        {
            return (file == null) ? NotFound() : file;
        }

    }//class
}
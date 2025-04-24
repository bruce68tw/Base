using Base.Enums;
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
        protected ContentResult JsonToCnt(JObject? json)
        {
            json ??= _Json.GetBrError("FindNone");
            return Content(json.ToString(), ContentTypeEstr.Json);
        }

        protected ContentResult JsonsToCnt(JArray? rows)
        {
            //如果傳回空字串前端會parser error !!
            string json = rows?.ToString(Newtonsoft.Json.Formatting.None) ?? "[]";
            return Content(json, ContentTypeEstr.Json);
        }

    }//class
}
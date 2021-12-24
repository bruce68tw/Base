using Base.Enums;
using Base.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace BaseApi.Controllers
{
    public class ApiCtrl : Controller 
    {
        //controller name
        public string Ctrl;

        //new way.
        public ApiCtrl()
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

        //json to content
        protected ContentResult JsonToCnt(JObject json)
        {
            if (json == null)
                json = _Json.GetBrError("FindNone");
            return Content(json.ToString(), ContentTypeEstr.Json);
        }

    }//class
}
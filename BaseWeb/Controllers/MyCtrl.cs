using Base.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;

namespace BaseWeb.Controllers
{
    public class MyCtrl : Controller 
    {
        //controller name
        public string Ctrl;

        override public void OnActionExecuting(ActionExecutingContext context)
        {
            //put constructor will not work !!
            Ctrl = ControllerContext.ActionDescriptor.ControllerName;
            base.OnActionExecuting(context);
        }

        //json to content
        protected ContentResult JsonToCnt(JObject json)
        {
            return Content(json.ToString(), ContentTypeEstr.Json);
        }

    }//class
}
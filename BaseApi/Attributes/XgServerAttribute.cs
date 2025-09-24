using Base.Enums;
using Base.Services;
using BaseApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace BaseApi.Attributes
{
    //檢查來源IP是否符合組態檔設定
    public class XgServerAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //check client ip
            var clientIp = _Http.GetIp();
            if (_Auth.CheckClientIp(clientIp))
            {
                //case of ok
                base.OnActionExecuting(context);
                return;
            }

            //log error
            _Log.Error($"Client IP 未授權: {clientIp}");

            //case of error below
            //get return type
            var error = _Locale.GetBaseRes().NoAuth;
            var returnType = (context.ActionDescriptor is ControllerActionDescriptor actor)
                ? actor.MethodInfo.ReturnType.Name
                : "ActionResult";    //default

            //return error
            if (returnType == "ActionResult")
            {
                //return error view
                context.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Error.cshtml",
                    //Model為唯讀, 必須用ViewData傳值 !!
                    ViewData = new ViewDataDictionary( new EmptyModelMetadataProvider(),
                        context.ModelState)
                    {
                        Model = error,
                    }
                };
            }
            else if (returnType == "JsonResult")
            {
                //4.return error model
                context.Result = new JsonResult(new
                {
                    Value = new { _ErrorMsg = error }
                });
            }
            else
            {
                //5.return error json(ContentResult)
                var json = _Json.GetError(error);
                context.Result = new ContentResult
                {
                    Content = _Json.ToStr(json),
                };
            }
        }
    } //class
}

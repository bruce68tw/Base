using Base.Enums;
using Base.Models;
using Base.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace BaseWeb.Attributes
{
    public class XgProgAuthAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// crud function type
        /// </summary>
        private CrudFunEnum _funType;

        public XgProgAuthAttribute(CrudFunEnum funType = CrudFunEnum.Empty)
        {
            _funType = funType;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //get controller name
            var ctrl = (string)context.RouteData.Values["Controller"];
            //var ctrl = context.Controller.ActionDescriptor.ControllerDescriptor.ControllerName;

            //=== check program right ===
            var userInfo = _Fun.GetBaseUser();
            var isLogin = userInfo.IsLogin;
            if (isLogin && _Prog.CheckAuth(userInfo.ProgAuthStrs, ctrl, _funType))
            {
                //case of ok
                base.OnActionExecuting(context);
                return;
            }

            //=== not login or no access right below ===
            //_Log.Error("No Permission: " + Prog + "->" + filterContext.ActionDescriptor.ActionName);

            //error msg when need
            //var msg = "您尚未有後台相關權限，請洽人事處進行權限申請。";
            var error = isLogin
                ? _Fun.GetBaseRes().NoProgAuth
                : _Fun.GetBaseRes().NotLogin;

            //get return type
            var returnType = "ActionResult";    //default
            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                returnType = controllerActionDescriptor.MethodInfo.ReturnType.Name;

            //1.return view
            if (returnType == "ActionResult")
            {
                if (!isLogin)
                {
                    //redirect to Home/Login action
                    context.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                            { "controller", "Home" },
                            { "action", "Login" }
                        });
                }
                else
                {
                    //return view of no access right.
                    context.Result = new ViewResult()
                    {
                        ViewName = "~/Views/Shared/NoProgAuth.cshtml",
                    };
                }
            }
            //2.return model
            else if (returnType == "JsonResult")
            {
                context.Result = new JsonResult(new
                {
                    Value = new ResultDto() { ErrorMsg = error }
                });
            }
            //3.return others
            //else if (type == typeof(ContentResult))
            else
            {
                //return error msg for client side
                var json = _Json.GetError(error);
                context.Result = new ContentResult()
                {
                    Content = _Json.ToStr(json),
                };
            }
        }

    } //class
}

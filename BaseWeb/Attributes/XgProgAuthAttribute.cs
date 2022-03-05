using Base.Enums;
using Base.Services;
using BaseWeb.Services;
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
        private readonly CrudEnum _crudEnum;

        public XgProgAuthAttribute(CrudEnum crudEnum = CrudEnum.Empty)
        {
            _crudEnum = crudEnum;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //1.check program right
            var ctrl = (string)context.RouteData.Values["Controller"];  //ctrl name
            var userInfo = _Fun.GetBaseUser();
            var isLogin = userInfo.IsLogin;
            if (isLogin && _XgProg.CheckAuth(userInfo.ProgAuthStrs, ctrl, _crudEnum))
            {
                //case of ok
                base.OnActionExecuting(context);
                return;
            }

            #region 2.set variables
            //_Log.Error("No Permission: " + Prog + "->" + filterContext.ActionDescriptor.ActionName);
            //error msg when need
            var error = isLogin
                ? _Locale.GetBaseRes().NoAuthProg
                : _Locale.GetBaseRes().NotLogin;

            //get return type
            var returnType = (context.ActionDescriptor is ControllerActionDescriptor actor)
                ? actor.MethodInfo.ReturnType.Name
                : "ActionResult";    //default
            #endregion

            //return error
            if (returnType == "ActionResult")
            {
                #region 3.return view: Login/NoProgAuth
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
                    context.Result = new ViewResult
                    {
                        ViewName = "~/Views/Shared/NoProgAuth.cshtml",
                    };
                }
                #endregion
            }
            else if (returnType == "JsonResult")
            {
                //4.return error model
                context.Result = new JsonResult(new
                {
                    Value = new { ErrorMsg = error }
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

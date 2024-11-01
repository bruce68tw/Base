﻿using Base.Enums;
using Base.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace BaseApi.Attributes
{
    public class XgProgAuthAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// crud function type
        /// </summary>
        private readonly CrudEnum _crudEnum;
        private readonly string _ctrl;

        /// <summary>
        /// check auth
        /// </summary>
        /// <param name="crudEnum"></param>
        /// <param name="ctrl">get controller name if empty</param>
        public XgProgAuthAttribute(CrudEnum crudEnum = CrudEnum.Empty, string ctrl = "")
        {
            _crudEnum = crudEnum;
            _ctrl = ctrl;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //1.check program right
            var ctrl = (_ctrl == "")
                ? (string)context.RouteData.Values["Controller"]  //ctrl name
                : _ctrl;
            var baseUser = _Fun.GetBaseUser();
            var isLogin = (baseUser.UserId != "");
            if (isLogin && _Auth.CheckAuth(baseUser.ProgAuthStrs, ctrl, _crudEnum))
            {
                //case of ok
                base.OnActionExecuting(context);
                return;
            }

            #region 2.set variables
            //_Log.Error("No Permission: " + Prog + "->" + filterContext.ActionDescriptor.ActionName);
            //error msg when need
            var error = isLogin
                ? _Str.GetBrError("NoAuthProg")   //"You can not Run this Program."
                : _Str.GetBrError("NotLogin");    //"Please Login First.";

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

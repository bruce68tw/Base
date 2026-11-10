using Base.Enums;
using Base.Services;
using BaseApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace BaseApi.Attributes
{
    /// <summary>
    /// 在 MVC Action 執行前檢查目前使用者是否具有指定作業與 CRUD 操作的權限。拒絕時依 Action 回傳型別選擇回應。
    /// </summary>
    /// <param name="crudEnum">要檢查的 CRUD 操作。預設值為 CrudEnum.Empty。</param>
    /// <param name="ctrl">要檢查的作業名稱，空字串時取路由的 Controller 名稱。</param>
    public class XgProgAuthAttribute(CrudEnum crudEnum = CrudEnum.Empty, string ctrl = "") : ActionFilterAttribute
    {
        /// <summary>
        /// crud function type
        /// </summary>
        private readonly CrudEnum _crudEnum = crudEnum;
        private readonly string _ctrl = ctrl;

        /// <summary>
        /// 檢查目前使用者的作業權限。未指定作業名稱時使用路由的 Controller 值，拒絕時設定 Action 結果。
        /// </summary>
        /// <param name="context">目前 MVC Action 的執行內容，提供路由、回傳型別及結果。</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //1.check program right
            var ctrl = (_ctrl == "")
                ? (string)context.RouteData.Values["Controller"]  //ctrl name
                : _ctrl;
            var baseUser = _Fun.GetBaseUser();
            var isLogin = (baseUser.UserId != "");
            if (isLogin && _Auth.CheckAuth(ctrl, _crudEnum, baseUser.ProgAuthStrs))
            {
                //case of ok
                base.OnActionExecuting(context);
                return;
            }

            //以下為無權限或未登入的處理

            #region 2.set variables
            //_Log.Error("No Permission: " + Prog + "->" + filterContext.ActionDescriptor.ActionName);
            //error msg when need
            var error = isLogin
                ? _Str.GetBrError(_Fun.FidNoAuthProg)   //"You can not Run this Program."
                : _Str.GetBrError(_Fun.FidNotLogin);    //"Please Login First.";

            //get return type
            /*
            var returnType = (context.ActionDescriptor is ControllerActionDescriptor actor)
                ? actor.MethodInfo.ReturnType.Name
                : "ActionResult";    //default
            */
            var returnType = "ActionResult";    //default
            var method = (context.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo;
            if (method != null)
            {
                var type = method.ReturnType;
                returnType = (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
                    ? type.GetGenericArguments()[0].Name
                    : type.Name;
            }
            #endregion

            //return error
            if (returnType == "ActionResult")
            {
                #region 3.return view: Login or Error
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
                        ViewName = "~/Views/Shared/_Error.cshtml",
                        //Model為唯讀, 必須用ViewData傳值 !!
                        ViewData = new ViewDataDictionary(
                            new EmptyModelMetadataProvider(),
                            context.ModelState)
                            {
                                Model = _Locale.GetBaseRes().NoAuth
                            }
                    };
                }
                #endregion
            }
            else if (returnType == "JsonResult")
            {
                //4.return error model
                context.Result = new JsonResult(new
                {
                    //Value = new { _ErrorMsg = error }
                    _ErrorMsg = error
                });
            }
            else if (returnType == "IActionResult")
            {
                //回傳檔案!!
                context.Result = new BadRequestObjectResult(new
                {
                    _ErrorMsg = error,
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

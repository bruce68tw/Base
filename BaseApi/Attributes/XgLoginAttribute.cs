using Base.Services;
using BaseApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace BaseApi.Attributes
{
    //如果未登入則導向登入頁面
    public class XgLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var baseUser = _Fun.GetBaseUser();
            if (baseUser.UserId == "")
            {
                //redirect to login action
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Home" },
                        { "action", "Login" },
                        { "url", _Http.GetWebPath() },
                    });
            }
            else
            {
                //case of ok
                base.OnActionExecuting(context);
            }
        }

    } //class
}

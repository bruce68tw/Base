using Base.Enums;
using Base.Models;
using Base.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace BaseWeb.Attributes
{
    public class XgLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var baseU = _Fun.GetBaseUser();
            if (baseU != null && baseU.IsLogin)
            {
                //case of ok
                base.OnActionExecuting(context);
                return;
            }
            else
            {
                //redirect to login action
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Home" },
                        { "action", "Login" }
                    });
            }
        }

    } //class
}

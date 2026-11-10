using Base.Services;
using BaseApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace BaseApi.Attributes
{
    //如果未登入則導向登入頁面
    /// <summary>
    /// 在 MVC Action 執行前檢查登入狀態。未登入時導向 Home/Login，並帶入原請求路徑與查詢字串。
    /// </summary>
    public class XgLoginAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 登入檢查通過時繼續執行 Action，否則導向登入頁並保留原請求路徑與查詢字串。
        /// </summary>
        /// <param name="context">目前 MVC Action 的執行內容，用於設定登入導向。</param>
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

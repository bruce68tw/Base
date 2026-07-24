using BaseApi.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //改成前端設定??
    public class XgCrudFunViewComponent : ViewComponent
    {
        public HtmlString Invoke(string key, string rowName = "", 
            bool hasUpdate = false, bool hasDelete = false, bool hasView = false,
            string fnOnUpdate = "", string fnOnDelete = "", string fnOnView = "")
        {
            var br = _Locale.GetBaseRes();
            var funs = "";
            if (hasUpdate)
                funs += $"<button type='button' class='btn btn-link' data-onclick='{(fnOnUpdate == "" ? "_me.crudR.onUpdate" : fnOnUpdate)}' data-args='{key}'><i class='ico-pen' title='{br.TipUpdate}'></i></button>";
            if (hasDelete)
                funs += $"<button type='button' class='btn btn-link' data-onclick='{(fnOnDelete == "" ? "_me.crudR.onDelete" : fnOnDelete)}' data-args='{key},{rowName}'><i class='ico-delete x-delete' title='{br.TipDelete}'></i></button>";
            if (hasView)
                funs += $"<button type='button' class='btn btn-link' data-onclick='{(fnOnView == "" ? "_me.crudR.onView" : fnOnView)}' data-args='{key}'><i class='ico-eye' title='{br.TipView}'></i></button>";
            return new HtmlString(funs);
        }
    }
}
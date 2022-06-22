using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XgCrudFunViewComponent : ViewComponent
    {
        public HtmlString Invoke(string key, string rowName = "", 
            bool hasUpdate = false, bool hasDelete = false, bool hasView = false,
            string fnOnUpdate = "", string fnOnDelete = "", string fnOnView = "")
        {
            var br = _Locale.GetBaseRes();
            var funs = "";
            if (hasUpdate)
                funs += string.Format("<button type='button' class='btn btn-link' onclick=\"{0}('{1}')\"><i class='ico-pen' title='{2}'></i></button>", ((fnOnUpdate == "") ? "_crudR.onUpdate" : fnOnUpdate), key, br.TipUpdate);
            if (hasDelete)
                funs += string.Format("<button type='button' class='btn btn-link' onclick=\"{0}('{1}','{2}')\"><i class='ico-delete' title='{3}'></i></button>", ((fnOnDelete == "") ? "_crudR.onDelete" : fnOnDelete), key, rowName, br.TipDelete);
            if (hasView)
                funs += string.Format("<button type='button' class='btn btn-link' onclick=\"{0}('{1}')\"><i class='ico-eye' title='{2}'></i></button>", ((fnOnView == "") ? "_crudR.onView" : fnOnView), key, br.TipView);

            return new HtmlString(funs);
        }
    }
}
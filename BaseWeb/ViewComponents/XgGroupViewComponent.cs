using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XgGroupViewComponent : ViewComponent
    {
        public HtmlString Invoke(string label, bool icon)
        {
            return new HtmlString(_Input.XgGroup(label, icon));
        }

    } //class
}
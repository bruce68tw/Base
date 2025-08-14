using BaseApi.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XgExportViewComponent : ViewComponent
    {
        public HtmlString Invoke(string fnOnClick = "_me.crudR.onExport")
        {
            //var rb = _Locale.RB;
            var html = $"<button type='button' class='btn btn-primary' data-onclick='{fnOnClick}'>{_Locale.GetBaseRes().BtnExport}<i class='ico-excel'></i></button>";
            return new HtmlString(html);
        }

    } //class
}
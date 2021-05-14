using Base.Services;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XgExportViewComponent : ViewComponent
    {
        public HtmlString Invoke(string fnOnClick = "_crud.onExport()")
        {
            //var rb = _Locale.RB;
            var html = "<button type='button' class='btn xg-btn-size btn-primary' onclick='{0}'>{1}<i class='ico-excel'></i></button>";
            return new HtmlString(string.Format(html, fnOnClick, _Locale.GetBaseRes().BtnExport));
        }

    } //class
}
using Base.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XgCreateViewComponent : ViewComponent
    {
        public HtmlString Invoke(string fnOnClick = "_crud.onCreate()")
        {
            //var rb = _Locale.RB;
            var html = "<button type='button' class='btn btn-success xg-btn-size' onclick='{0}'>{1}<i class='ico-plus'></i></button>";
            return new HtmlString(string.Format(html, fnOnClick, _Fun.GetBaseRes().BtnCreate));
        }

    } //class
}
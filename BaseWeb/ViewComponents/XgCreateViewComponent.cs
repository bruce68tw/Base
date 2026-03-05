using BaseApi.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XgCreateViewComponent : ViewComponent
    {
        public HtmlString Invoke(string fnOnClick = "_me.crudR.onCreate")
        {
            var label = _Locale.GetBaseRes().BtnCreate;
            //label和icon之間不要斷行, 否則會產生空格
            return new HtmlString($@"
<button type='button' class='btn x-btn2 xd-create' data-onclick='{fnOnClick}'>{label}<i class='ico-plus'></i>
</button>");
        }
    }
}
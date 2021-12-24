using Base.Services;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //add row button
    public class XgDeleteRowViewComponent : ViewComponent
    {
        public HtmlString Invoke(string fnOnClick)
        {
            //var rb = _Locale.RB;
            var html = string.Format(@"
<button type='button' onclick='{0}' class='btn btn-link'>
    <i class='ico-delete' title='{1}'></i>
</button>", fnOnClick, _Locale.GetBaseRes().TipDeleteRow);

            return new HtmlString(html);
        }        

    }//class
}

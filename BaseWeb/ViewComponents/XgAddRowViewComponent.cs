using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //add row button
    public class XgAddRowViewComponent : ViewComponent
    {
        public HtmlString Invoke(string fnOnClick)
        {
            var html = string.Format(@"
<button type='button' onclick='{0}' class='btn btn-success xg-btn-size'>{1}
    <i class='ico-plus'></i>
</button>
", fnOnClick, _Locale.GetBaseRes().BtnAddRow);

            return new HtmlString(html);
        }
    }//class
}

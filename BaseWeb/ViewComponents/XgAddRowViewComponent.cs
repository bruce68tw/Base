using BaseApi.Services;
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
            //todo:
            var attr = _Input.GetEventAttr("onclick", fnOnClick);
            var html = $@"
<button type='button' {attr} class='btn btn-success'>{_Locale.GetBaseRes().BtnAddRow}
    <i class='ico-plus'></i>
</button>
";
            return new HtmlString(html);
        }
    }//class
}

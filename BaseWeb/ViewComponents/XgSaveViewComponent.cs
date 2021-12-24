using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XgSaveViewComponent : ViewComponent
    {
        public HtmlString Invoke(string align = "center", string fnOnSave = "_crud.onSave()")
        {
            var baseR = _Locale.GetBaseRes();
            var html = $@"
<div class='xg-{align}'>
    <button id='btnSave' type='button' class='btn xg-btn-size btn-success' onclick='{fnOnSave}'>{baseR.BtnSave}<i class='ico-save'></i></button>
</div>
";
            return new HtmlString(html);
        }

    } //class
}
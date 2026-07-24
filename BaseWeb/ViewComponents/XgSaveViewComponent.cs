using BaseApi.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XgSaveViewComponent : ViewComponent
    {
        public HtmlString Invoke(string align = "center", string fnOnSave = "_me.crudE.onSave()")
        {
            var baseR = _Locale.GetBaseRes();
            var html = $@"
<div class='x-{align}'>
    <button id='btnSave' type='button' class='btn x-btn1' data-edit data-onclick='{fnOnSave}'>{baseR.BtnSave}<i class='ico-save'></i></button>
</div>
";
            return new HtmlString(html);
        }

    } //class
}
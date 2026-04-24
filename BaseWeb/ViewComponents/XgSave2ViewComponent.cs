using BaseApi.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //XgSaveBack -> XgSave2
    public class XgSave2ViewComponent : ViewComponent
    {
        public HtmlString Invoke(string align = "center", string fnOnSave = "_me.crudE.onSaveA", string fnOnBack = "_me.crudR.onToRead" )
        {
            var baseR = _Locale.GetBaseRes();
            var html = $@"
<div class='x-btns-box x-{align}'>
    <button id='btnToRead' type='button' class='btn x-btn-cancel' data-onclick='{fnOnBack}'>{baseR.BtnToRead}<i class='ico-back'></i></button>
    <button id='btnSave' type='button' class='btn x-btn1' data-edit data-onclick='{fnOnSave}'>{baseR.BtnSave}<i class='ico-save'></i></button>
</div>
";
            return new HtmlString(html);
        }

    } //class
}
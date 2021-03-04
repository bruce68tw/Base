using Base.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XgSaveBackViewComponent : ViewComponent
    {
        public HtmlString Invoke(string align = "center", string fnOnSave = "_crud.onSave()", string fnOnBack = "_crud.onToRead()" )
        {
            var baseR = _Fun.GetBaseRes();
            var html = $@"
<div class='xg-{align}'>
    <button id='btnSave' type='button' class='btn xg-btn-size btn-success' onclick='{fnOnSave}'>{baseR.BtnSave}<i class='ico-save'></i></button>
    <button id='btnToRead' type='button' class='btn xg-btn-size btn-primary' onclick='{fnOnBack}'>{baseR.BtnToRead}<i class='ico-back'></i></button>
</div>
";
            return new HtmlString(html);
        }

    } //class
}
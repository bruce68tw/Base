﻿using BaseApi.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XgSaveViewComponent : ViewComponent
    {
        public HtmlString Invoke(string align = "center", string fnOnSave = "_me.crudE.onSaveA()")
        {
            var baseR = _Locale.GetBaseRes();
            var html = $@"
<div class='xg-{align}'>
    <button id='btnSave' type='button' class='btn btn-success' onclick='{fnOnSave}'>{baseR.BtnSave}<i class='ico-save'></i></button>
</div>
";
            return new HtmlString(html);
        }

    } //class
}
﻿using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XgCreateViewComponent : ViewComponent
    {
        public HtmlString Invoke(string fnOnClick = "_me.crudR.onCreate()")
        {
            var label = _Locale.GetBaseRes().BtnCreate;
            return new HtmlString($@"
<button type='button' class='btn btn-success xd-create' onclick='{fnOnClick}'>
    {label}
    <i class='ico-plus'></i>
</button>");
        }
    }
}
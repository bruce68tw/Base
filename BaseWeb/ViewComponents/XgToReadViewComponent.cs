using BaseApi.Services;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //add row button
    public class XgToReadViewComponent : ViewComponent
    {
        public HtmlString Invoke(string label = "")
        {
            if (string.IsNullOrEmpty(label))
                label = _Locale.GetBaseRes().BtnToRead;

            var html = $@"
<button id='btnToRead' type='button' class='btn x-btn-cancel' data-onclick='_me.crudR.onToRead'>{label}
    <i class='ico-back'></i>
</button>
";
            return new HtmlString(html);
        }
    }//class
}

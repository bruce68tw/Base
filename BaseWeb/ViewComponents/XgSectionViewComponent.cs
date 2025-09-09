using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XgSectionViewComponent : ViewComponent
    {
        public HtmlString Invoke(string label)
        {
            var html = $"<div class='x-section'>{label}</div>";
            return new HtmlString(html);
        }

    } //class
}
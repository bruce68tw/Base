using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BaseWeb.Helpers
{
    //field title, no row container
    //title is before field, label is after field
    public static class XgTitleHelper
    {
        public static IHtmlContent XgTitle(this IHtmlHelper htmlHelper, string label, bool required = false, int cols = 2)
        {
            var reqStr = required ? "<span class='xg-required'>*</span>" : "";
            var html = string.Format(@"
<div class='col-md-{1} xg-label'>{0}
    {2}
</div>
", label, cols, reqStr);

            return new HtmlString(html);
        }

    }//class
}

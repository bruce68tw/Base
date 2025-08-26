using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XgProgPathViewComponent : ViewComponent
    {
        /// <summary>
        /// program path
        /// </summary>
        /// <param name="path">prog path list</param>
        /// <returns></returns>
        public HtmlString Invoke(string path = "")
        {
            var html = $"<div class='x-prog-path'>{path}</div>";
            return new HtmlString(html);
        }

    } //class
}
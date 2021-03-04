using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XgProgPathViewComponent : ViewComponent
    {
        /// <summary>
        /// program path
        /// </summary>
        /// <param name="names">prog path list</param>
        /// <returns></returns>
        public HtmlString Invoke(string[] names)
        {
            var list = names[0];
            for (var i=1; i< names.Length; i++)
                list += " / " + names[i];

            var html = "<div class='xg-prog-path'>{0}</div>";
            return new HtmlString(string.Format(html, list));
        }

    } //class
}
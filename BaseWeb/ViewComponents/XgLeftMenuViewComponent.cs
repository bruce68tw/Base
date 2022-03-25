using Base.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace BaseWeb.ViewComponents
{

    public class XgLeftMenuViewComponent : ViewComponent
    {
        /// <summary>
        /// left side menu(max to 2 levels), outside class name is xg-leftmenu
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="rows">menu rows</param>
        /// <param name="minWidth">px</param>
        /// <param name="maxWidth">px</param>
        /// <returns></returns>
        public HtmlString Invoke(List<MenuDto> rows, bool showSort = false)
        {
            if (showSort)
            {
                rows.ForEach(a => {
                    a.Name = a.Sort + "." + a.Name;
                });
            }

            var html = "";
            foreach (var row in rows)
            {
                //no sub menu
                if (row.Items == null || row.Items.Count == 0)
                {
                    html += string.Format(@"
<li>
    <a href='{0}' data-pjax>
        <i class='{1}'></i>{2}
    </a>
</li>
", row.Url, row.Icon, row.Name);
                }
                //has sub menu
                else
                {
                    var childs = "";
                    foreach (var item in row.Items)
                    {
                        childs += string.Format(@"
<li><a href='{0}' data-pjax>{1}</a></li>
", item.Url, item.Name);
                    }

                    html += string.Format(@"
<li>
    <a class='collapsed xg-toggle'>
        <i class='{0}'></i>{1}
        <b class='xg-arrow'></b>
    </a>
    <ul class='collapse xg-leftmenu-panel' role='menu'>
        {2}
    </ul>
</li>
", row.Icon, row.Name, childs);

                }//if
            }//for

            html = "<ul class='xg-leftmenu'>" + html + "</ul>";
            return new HtmlString(html);
        }

    }//class
}

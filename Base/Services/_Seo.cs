using HandlebarsDotNet;
using Newtonsoft.Json.Linq;
using System.Web;

namespace Base
{
    //SEO
    public class _Seo
    {
        //instance variables
        //private static DateTime _start;
        //private const string _newLine = "\r\n";

        /// <summary>
        /// 產生 seo siteMap.xml 檔案
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="rows">包含Loc, LastMod 2個欄位</param>
        /// <param name="changeFreq"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public static byte[]? GenSiteMapFile(string domain, JArray? rows, string changeFreq = "weekly", string priority = "1.0") 
        {
            if (rows == null) return null;

            //handleBars 多筆區域內只能使用自己的欄位
            foreach(JObject row in rows)
            {
                row["Loc"] = domain + "/" + row["Loc"];
                row["ChangeFreq"] = changeFreq;
                row["Priority"] = priority;
            }

            //siteMap file template
            var fileTpl = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
    {{#each Items}}
    <url>
        <loc>{{Loc}}</loc>
        <lastmod>{{LastMod}}</lastmod>
        <changefreq>{{ChangeFreq}}</changefreq>
        <priority>{{Priority}}</priority>
    </url>
    {{/each}}
</urlset>
";
            var json = new { Items = rows };
            var mustache = Handlebars.Compile(fileTpl);
            var result = HttpUtility.HtmlDecode(mustache(json).ToString().Trim());
            return System.Text.Encoding.UTF8.GetBytes(result);
        }

    }
}

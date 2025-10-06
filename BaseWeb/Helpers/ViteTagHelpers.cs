using BaseWeb.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BaseWeb.Helpers
{
    [HtmlTargetElement("vite-script", Attributes = "entry")]
    public class ViteScriptTagHelper : TagHelper
    {
        public string Entry { get; set; } = "";

        private readonly IWebHostEnvironment _env;

        public ViteScriptTagHelper(IWebHostEnvironment env)
        {
            _env = env;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var src = _Vite.GetAssetPath(Entry, _env);
            output.TagName = "script";
            output.Attributes.SetAttribute("type", "module");
            output.Attributes.SetAttribute("src", src);
        }
    }

    [HtmlTargetElement("vite-style", Attributes = "entry")]
    public class ViteStyleTagHelper : TagHelper
    {
        public string Entry { get; set; } = "";

        private readonly IWebHostEnvironment _env;

        public ViteStyleTagHelper(IWebHostEnvironment env)
        {
            _env = env;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var href = _Vite.GetAssetPath(Entry, _env);
            output.TagName = "link";
            output.Attributes.SetAttribute("rel", "stylesheet");
            output.Attributes.SetAttribute("href", href);
        }
    }
}
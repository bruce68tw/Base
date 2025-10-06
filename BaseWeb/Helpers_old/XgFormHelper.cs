using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace BaseWeb.Helpers
{
    //see: http://www.nogginbox.co.uk/blog/mvc-html-wrapper-helper
    public static class XgFormHelper
    {
        //header
        public static XgForm BeginXgForm(this IHtmlHelper helper)
        {
            var html = @"
<form class='form-body' id='formEdit'>
";

            helper.ViewContext.Writer.Write(html);
            return new XgForm(helper);
        }

        //tailer
        public static void EndXgForm(this IHtmlHelper helper)
        {
            var html = @"
</form>
";
            helper.ViewContext.Writer.Write(html);
        }
    }//class(XgFormHelper)


    public class XgForm : IDisposable
    {
        private readonly IHtmlHelper _helper;
        public XgForm(IHtmlHelper helper)
        {
            _helper = helper;
        }
        public void Dispose()
        {
            _helper.EndXgForm();
        }
    }//class(XpForm)
}

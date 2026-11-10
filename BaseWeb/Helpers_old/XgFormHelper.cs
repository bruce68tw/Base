using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace BaseWeb.Helpers
{
    //see: http://www.nogginbox.co.uk/blog/mvc-html-wrapper-helper
    /// <summary>提供開啟及結束表單的舊版 Razor 輔助方法。</summary>
    public static class XgFormHelper
    {
        //header
        /// <summary>建立表單範圍，供 using 結束時輸出表單尾標籤。</summary>
        /// <param name="helper">目前 Razor 檢視使用的 HTML 輔助物件</param>
        public static XgForm BeginXgForm(this IHtmlHelper helper)
        {
            var html = @"
<form class='form-body' id='formEdit'>
";

            helper.ViewContext.Writer.Write(html);
            return new XgForm(helper);
        }

        //tailer
        /// <summary>向目前 Razor 輸出寫入表單尾標籤。</summary>
        /// <param name="helper">目前 Razor 檢視使用的 HTML 輔助物件</param>
        public static void EndXgForm(this IHtmlHelper helper)
        {
            var html = @"
</form>
";
            helper.ViewContext.Writer.Write(html);
        }
    }//class(XgFormHelper)


    /// <summary>管理舊版 Razor 表單的起訖輸出。</summary>
    public class XgForm : IDisposable
    {
        private readonly IHtmlHelper _helper;
        /// <summary>保存 Razor 輔助物件，供釋放時寫入表單尾標籤。</summary>
        /// <param name="helper">目前 Razor 檢視使用的 HTML 輔助物件</param>
        public XgForm(IHtmlHelper helper)
        {
            _helper = helper;
        }
        /// <summary>向 Razor 輸出寫入表單尾標籤。</summary>
        public void Dispose()
        {
            _helper.EndXgForm();
        }
    }//class(XpForm)
}

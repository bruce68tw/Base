using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>呈現HTML 編輯欄位的 MVC ViewComponent。</summary>
    public class XiHtmlViewComponent : ViewComponent
    {
        /// <summary>產生HTML 編輯欄位的 HTML。</summary>
        /// <param name="dto">用於產生HTML 編輯欄位的欄位設定。</param>
        public HtmlString Invoke(XiHtmlDto dto)
        {
            return new HtmlString(_Input.XiHtml(dto));
        }

    } //class
}
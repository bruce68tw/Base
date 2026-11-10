using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>呈現連結欄位的 MVC ViewComponent。</summary>
    public class XiLinkViewComponent : ViewComponent
    {
        /// <summary>產生連結欄位的 HTML。</summary>
        /// <param name="dto">用於產生連結欄位的欄位設定。</param>
        public HtmlString Invoke(XiLinkDto dto)
        {
            return new HtmlString(_Input.XiLink(dto));
        }
        
    } //class
}

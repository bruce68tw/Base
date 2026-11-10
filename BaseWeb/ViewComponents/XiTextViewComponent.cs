using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>呈現單行文字欄位的 MVC ViewComponent。 (相鄰位置)</summary>
    public class XiTextViewComponent : ViewComponent
    {
        /// <summary>產生單行文字欄位的 HTML。</summary>
        /// <param name="dto">用於產生單行文字欄位的欄位設定。</param>
        public HtmlString Invoke(XiTextDto dto)
        {
            return new HtmlString(_Input.XiText(dto));
        } 

    }
}

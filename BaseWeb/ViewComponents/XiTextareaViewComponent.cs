using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //textarea input
    /// <summary>呈現多行文字欄位的 MVC ViewComponent。</summary>
    public class XiTextareaViewComponent : ViewComponent
    {
        /// <summary>產生多行文字欄位的 HTML。</summary>
        /// <param name="dto">用於產生多行文字欄位的欄位設定。</param>
        public HtmlString Invoke(XiTextareaDto dto)
        {
            return new HtmlString(_Input.XiTextarea(dto));
        }

    } //class
}
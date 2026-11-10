using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>呈現核取欄位的 MVC ViewComponent。</summary>
    public class XiCheckViewComponent : ViewComponent
    {
        /// <summary>產生核取欄位的 HTML。</summary>
        /// <param name="dto">用於產生核取欄位的欄位設定。</param>
        /// <returns></returns>
        public HtmlString Invoke(XiCheckDto dto)
        {
            return new HtmlString(_Input.XiCheck(dto));
        }

    }//class
}
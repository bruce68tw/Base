using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>呈現附文字輸入的核取欄位的 MVC ViewComponent。</summary>
    public class XiCheckTextViewComponent : ViewComponent
    {
        /// <summary>產生附文字輸入的核取欄位的 HTML。</summary>
        /// <param name="dto">用於產生附文字輸入的核取欄位的欄位設定。</param>
        /// <returns></returns>
        public HtmlString Invoke(XiCheckTextDto dto)
        {
            return new HtmlString(_Input.XiCheckText(dto));
        }

    }//class
}
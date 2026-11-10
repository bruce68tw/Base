using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>呈現附文字輸入的單選欄位的 MVC ViewComponent。</summary>
    public class XiRadioTextViewComponent : ViewComponent
    {
        /// <summary>產生附文字輸入的單選欄位的 HTML。</summary>
        /// <param name="dto">用於產生附文字輸入的單選欄位的欄位設定。</param>
        /// <returns></returns>
        public HtmlString Invoke(XiRadioTextDto dto)
        {
            return new HtmlString(_Input.XiRadioText(dto));
        }

    }//class
}
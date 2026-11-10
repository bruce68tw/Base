using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>呈現隱藏輸入欄位的 MVC ViewComponent。</summary>
    public class XiHideViewComponent : ViewComponent
    {
        /// <summary>產生隱藏輸入欄位的 HTML。</summary>
        /// <param name="dto">用於產生隱藏輸入欄位的欄位設定。</param>
        public HtmlString Invoke(XiHideDto dto)
        {
            return new HtmlString(_Input.XiHide(dto));
        }

    }//class
}

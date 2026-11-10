using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;

namespace BaseWeb.ViewComponents
{
    //readonly field
    /// <summary>呈現唯讀顯示欄位的 MVC ViewComponent。</summary>
    public class XiReadViewComponent
    {
        /// <summary>產生唯讀顯示欄位的 HTML。</summary>
        /// <param name="dto">用於產生唯讀顯示欄位的欄位設定。</param>
        public HtmlString Invoke(XiReadDto dto)
        {
            return new HtmlString(_Input.XiRead(dto));
        }

    }
}

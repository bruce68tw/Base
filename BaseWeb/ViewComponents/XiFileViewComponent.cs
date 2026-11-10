using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>呈現檔案上傳欄位的 MVC ViewComponent。</summary>
    public class XiFileViewComponent : ViewComponent
    {
        /// <summary>產生檔案上傳欄位的 HTML。</summary>
        /// <param name="dto">用於產生檔案上傳欄位的欄位設定。</param>
        /// <returns></returns>
        public HtmlString Invoke(XiFileDto dto)
        {
            return new HtmlString(_Input.XiFile(dto));
        }
        
    } //class
}

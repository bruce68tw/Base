using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>
    /// file input
    /// </summary>
    public class XiFileViewComponent : ViewComponent
    {
        /// <summary>
        /// file upload, 
        /// </summary>
        /// <returns></returns>
        public HtmlString Invoke(XiFileDto dto)
        {
            return new HtmlString(_Input.XiFile(dto));
        }
        
    } //class
}

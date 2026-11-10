using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //select input
    /// <summary>呈現下拉選擇欄位的 MVC ViewComponent。</summary>
    public class XiSelectViewComponent : ViewComponent
    {
        /// <summary>產生下拉選擇欄位的 HTML。</summary>
        /// <param name="dto">用於產生下拉選擇欄位的欄位設定。</param>
        public HtmlString Invoke(XiSelectDto dto)
        {
            return new HtmlString(_Input.XiSelect(dto));
        } 

    }//class
}

using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

//TODO: modifing
namespace BaseWeb.ViewComponents
{
    //Radio button group, consider horizontal or vertical
    /// <summary>呈現單選選項群組的 MVC ViewComponent。</summary>
    public class XiRadioViewComponent : ViewComponent
    {
        /// <summary>產生單選選項群組的 HTML。</summary>
        /// <param name="dto">用於產生單選選項群組的欄位設定。</param>
        public HtmlString Invoke(XiRadioDto dto)
        {
            return new HtmlString(_Input.XiRadio(dto));
        }

    }//class
}

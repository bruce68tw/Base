using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //integer/decimal input(3 different: class name, input arg, digit=true !!)
    /// <summary>呈現整數輸入欄位的 MVC ViewComponent。</summary>
    public class XiIntViewComponent : ViewComponent
    {
        /// <summary>產生整數輸入欄位的 HTML。</summary>
        /// <param name="dto">用於產生整數輸入欄位的欄位設定。</param>
        public HtmlString Invoke(XiIntDto dto)
        {
            return new HtmlString(_Input.XiInt(dto));
        } 

    }//class
}

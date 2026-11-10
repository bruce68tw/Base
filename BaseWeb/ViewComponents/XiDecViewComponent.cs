using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //integer/decimal input(3 different: class name, input arg, digit=true !!)
    /// <summary>呈現小數輸入欄位的 MVC ViewComponent。</summary>
    public class XiDecViewComponent : ViewComponent
    {
        /// <summary>產生小數輸入欄位的 HTML。</summary>
        /// <param name="dto">用於產生小數輸入欄位的欄位設定。</param>
        public HtmlString Invoke(XiDecDto dto)
        {
            return new HtmlString(_Input.XiDec(dto));
        } 

    }//class
}

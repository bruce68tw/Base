using Base.Services;
using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.Helpers
{
    //use bootstrap datepicker
    /// <summary>呈現日期輸入欄位的 MVC ViewComponent。</summary>
    public class XiDateViewComponent : ViewComponent
    {
        /// <summary>產生日期輸入欄位的 HTML。</summary>
        /// <param name="dto">用於產生日期輸入欄位的欄位設定。</param>
        /// <returns></returns>
        public HtmlString Invoke(XiDateDto dto)
        {
            return new HtmlString(_Input.XiDate(dto));
        } 

    }//calss
}

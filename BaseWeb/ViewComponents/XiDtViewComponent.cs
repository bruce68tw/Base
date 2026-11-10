using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.Helpers
{
    //use bootstrap datepicker
    /// <summary>呈現日期與時間輸入欄位的 MVC ViewComponent。</summary>
    public class XiDtViewComponent : ViewComponent
    {
        /// <summary>產生日期與時間輸入欄位的 HTML。</summary>
        /// <param name="dto">用於產生日期與時間輸入欄位的欄位設定。</param>
        public HtmlString Invoke(XiDtDto dto)
        {
            return new HtmlString(_Input.XiDt(dto));
        } 

    }//calss
}

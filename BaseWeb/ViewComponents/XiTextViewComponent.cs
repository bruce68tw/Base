using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>
    /// text input
    /// xi-box用來顯示 validation error(相鄰位置), 大多數欄位的xi-box即為輸入欄位
    /// BoxClass會加到 xi-box上面
    /// </summary>
    public class XiTextViewComponent : ViewComponent
    {
        public HtmlString Invoke(XiTextDto dto)
        {
            return new HtmlString(_Input.XiText(dto));
        } 

    }
}

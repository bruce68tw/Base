using Base.Services;
using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>
    /// link file
    /// </summary>
    public class XiLinkViewComponent : ViewComponent
    {
        public HtmlString Invoke(XiLinkDto dto)
        {
            if (string.IsNullOrEmpty(dto.FnOnClick))
                dto.FnOnClick = $"_me.onViewFile(\"{dto.Fid}\", this)";
            dto.FnOnClick = _Helper.GetLinkFn(dto.FnOnClick);

            var html = $@"
<a href='#' data-fid='{dto.Fid}' data-type='linkFile' class='{dto.ExtClass}' 
    style='height:32px; display:table-cell; vertical-align:middle;' 
    onclick='{dto.FnOnClick}' {dto.ExtAttr}>{dto.Value}
</a>
";
            //add title if need
            if (!string.IsNullOrEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, false, dto.LabelTip, dto.InRow, dto.Cols);
            return new HtmlString(html);
        }
        
    } //class
}

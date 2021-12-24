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
            if (_Str.IsEmpty(dto.FnOnViewFile))
                dto.FnOnViewFile = $"_me.onViewFile(\"{dto.Table}\", \"{dto.Fid}\", this)";
            dto.FnOnViewFile = _Helper.GetLinkFn(dto.FnOnViewFile);

            //add class xi-unsave for not save Db !!
            var attr = _Helper.GetInputAttr(dto.Fid, "", false, dto.InputAttr) +
                $" data-type='link' onclick='{dto.FnOnViewFile}'" +
                " style='height:32px; display:table-cell; vertical-align:middle;'";
            var html = $"<a href='#' {attr} class='xi-unsave {dto.BoxClass}'>{dto.Value}</a>";

            //add title if need
            if (!_Str.IsEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, false, dto.LabelTip, dto.InRow, dto.Cols);
            return new HtmlString(html);
        }
        
    } //class
}

using Base.Services;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //open modal for table
    //link with button style(always enabled)
    public class XgOpenModalViewComponent : ViewComponent
    {
        public HtmlString Invoke(string title, string fid, bool required, int maxLen)
        {
            var btnOpen = _Locale.GetBaseRes().BtnOpen;
            var req = required ? "true" : "false";
            var html = $"<a onclick='_crud.onOpenModal(this, \"{title}\", \"{fid}\", {req}, {maxLen})' class='btn btn-outline-secondary btn-sm'>{btnOpen}</a>";
            return new HtmlString(html);
        }        

    }//class
}

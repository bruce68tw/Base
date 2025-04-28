using Base.Services;
using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.Helpers
{
    //use bootstrap datepicker
    public class XiDtViewComponent : ViewComponent
    {
        //fids:0(date),1(hour),2(minute)
        public HtmlString Invoke(XiDtDto dto)
        {
            string date = "", hour = "", min = "";
            if (_Str.NotEmpty(dto.Value))
            {
                var dt = _Date.CsToDt(dto.Value)!.Value;
                date = dt.Date.ToString();
                hour = dt.Hour.ToString();
                min = dt.Minute.ToString();
            }

            var attr = "style='width:70px; display:inline-block;'";
            var html = string.Format(@"
<div data-fid='{0}' data-type='dt' class='xi-box {1}' {2}>
    {3}
    {4}
    <span>:</span>
    {5}
</div>",
dto.Fid, dto.BoxClass, dto.InputAttr,
_Helper.GetDateHtml("", date, "", dto.Required, dto.Edit, dto.InputTip),
_Helper.GetSelectHtml("", hour, "", _Date.GetHourList(), false, dto.Edit, false, inputAttr: attr),
_Helper.GetSelectHtml("", min, "", _Date.GetMinuteList(dto.MinuteStep), false, dto.Edit, false, inputAttr: attr)
);

            if (_Str.NotEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, dto.Required, dto.LabelTip, dto.InRow, dto.Cols ?? "2,5");
            return new HtmlString(html);
        } 

    }//calss
}

using Base.Services;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.Helpers
{
    //use bootstrap datepicker
    public class XiDtViewComponent : ViewComponent
    {
        //fids:0(date),1(hour),2(minute)
        public HtmlString Invoke(string title, string fid, string value = "",
            bool editable = true, bool inRow = false, bool required = false,
            string labelTip = "", string inputTip = "", string extAttr = "", string extClass = "",
            int minuteStep = 10,
            string cols = "")
        {

            string date = "", hour = "", min = "";
            if (value != "")
            {
                var dt = _Date.CsToDt(value).Value;
                date = dt.Date.ToString();
                hour = dt.Hour.ToString();
                min = dt.Minute.ToString();
            }

            var width = "style='width:70px'";
            var html = string.Format(@"
<div data-fid='{0}' data-type='dt' class='xi-dt {1}' {2}>
    {3}
    {4}
    <span>:</span>
    {5}
</div>",
fid, extClass, extAttr,
_Helper.GetDateHtml("", date, "", required, editable, inputTip, extClass: "xg-inline"),
_Helper.GetSelectHtml("", hour, "", _Date.GetHourList(), false, false, false, editable, false, extAttr: width, extClass: "xg-inline"),
_Helper.GetSelectHtml("", min, "", _Date.GetMinuteList(minuteStep), false, false, false, editable, false, extAttr: width, extClass: "xg-inline")
);

            if (!string.IsNullOrEmpty(title))
                html = _Helper.InputAddLayout(html, title, required, labelTip, inRow, cols);

            return new HtmlString(html);
        } 

    }//calss
}

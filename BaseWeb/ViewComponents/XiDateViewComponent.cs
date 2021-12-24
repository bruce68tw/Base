using Base.Services;
using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.Helpers
{
    //use bootstrap datepicker
    public class XiDateViewComponent : ViewComponent
    {
        /// <summary>
        /// date field
        /// </summary>
        /// <returns></returns>
        public HtmlString Invoke(XiDateDto dto)
        {
            /*
             string title, string fid, string value = "",
            string edit = "", bool inRow = false, bool required = false,
            string labelTip = "", string inputTip = "", string extAttr = "", 
            string extClass = "", string cols = ""
             */
            var html = _Helper.GetDateHtml(dto.Fid, dto.Value, "date",
                dto.Required, dto.Edit, dto.InputTip, dto.InputAttr, dto.BoxClass);
            if (!_Str.IsEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, dto.Required, dto.LabelTip, dto.InRow, dto.Cols);

            return new HtmlString(html);

            /* 
            //使用 bootstrap-datepicker-mobile 無法寫入初始值, 先不開啟此功能 !!
            if (_Device.IsMobile())
            {
                html = @"
<input type='text' class='date-picker form-control' id='{0}' name='{0}' value='{1}' placeholder=""{2}""
    data-date-format='{5}' data-date='{1}' />
<span id='{3}' class='{4}'></span>
";

            }
            else
            {
            */

            /*
            html = @"
<div class='input-group date datepicker' data-date-format='{5}' style='padding:0px; border-radius:3px;'>
    <input type='text' id='{0}' name='{0}' value='{1}' class='form-control' placeholder=""{2}"">
    <div class='input-group-addon'>
        <i class='fa fa-calendar' aria-hidden='true'></i>
    </div>
</div>
<span id='{3}' class='{4}'></span>
";
            //}
            //暫解
            if (value.Length > 10)
                value = value.Substring(0, 10).TrimEnd();

            //把日期轉換成為所需要的格式
            html = String.Format(html, fid, value, placeHolder, fid + _WebFun.Error, _WebFun.ErrorLabelClass, _Locale.DateFormatFront);
            return new HtmlString(html);
            */
        } 

    }//calss
}

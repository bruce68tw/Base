using System;
using System.Linq.Expressions;
using Base.Services;
using BaseWeb.Services;

//todo
namespace BaseWeb.Helpers
{
    public static class XiTimeHelper
    {
        /*
        /// <summary>
        /// 時間欄位含 error msg
        /// 20161216 Bruce
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="fid"></param>
        /// <param name="value"></param>
        /// <param name="placeHolder"></param>
        /// <returns></returns>
        public static IHtmlContent XiTime(this IHtmlHelper htmlHelper, string fid, string value = "", string placeHolder="")
        {
            return GetStr(fid, value, placeHolder);
        }

        /// <summary>
        /// binding model
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IHtmlContent XiTimeFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string placeHolder = "")
        {
            //讀取欄位 id, value
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var fid = metadata.PropertyName;
            var value = metadata.Model == null ? "" : metadata.Model.ToString();
            return GetStr(fid, value, placeHolder);
        }

        private static IHtmlContent GetStr(string fid, string value, string placeHolder)
        {
            //pc, 手機使用不同的UI
            //placeholder可能會包含單引號, 所以escape處理(")
            var html = "";
            if (_Device.IsMobile())
            {
                html = @"
<input type='time' id='{0}' name='{0}' value='{1}' />
<span id='{3}' class='{4}'></span>
";

                html = String.Format(html, fid, value, placeHolder, fid + _WebFun.ErrTail, _WebFun.ErrLabCls, _Locale.DateFormatFront);
            }
            else
            {
                //pc版 UI
                html = @"
<div style='width:100%'>
    <select id='{0}' name='{0}' {4} >
        {1}
    </select>
    <span id='{2}' class='{3}'></span>
</div>
";
                //get 下拉式欄位清單
                string[] values = new string[48];
                var i = 0;
                for (i = 0; i < 24; i=i+2)
                {
                    values[i] = i + ":00 am";
                    values[i+1] = i + ":30 am";
                    values[24+i] = i + ":00 pm";
                    values[25+i] = i + ":30 pm";
                }

                //get list string
                var list = "";
                for (i = 0; i < 48; i++)
                {
                    var selected = (value == values[i]) ? " selected " : "";
                    list += "<option value='" + values[i] + "'" + selected + ">" + values[i] + "</option>";
                }

                //get select property string
                var prop2 = _Helper.GetSelectProp(null);
                html = String.Format(html, fid, list, fid + _WebFun.ErrTail, _WebFun.ErrLabCls, prop2);
            }

            return new HtmlString(html);
        } 
        */

    }//class
}

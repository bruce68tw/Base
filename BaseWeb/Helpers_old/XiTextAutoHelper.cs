using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using BaseWeb.Services;

//todo
namespace BaseWeb.Helpers
{
    /// <summary>提供舊版自動完成文字欄位輔助方法。</summary>
    public static class XiTextAutoHelper
    {

        /// <summary>產生可由前端初始化自動完成的文字輸入欄位。</summary>
        /// <param name="htmlHelper">目前 Razor 檢視使用的 HTML 輔助物件</param>
        /// <param name="fid">要處理的欄位識別值</param>
        /// <param name="hint">文字輸入欄位的提示文字</param>
        /// <param name="value">用於初始化欄位或選項的值</param>
        /// <param name="maxLength">文字長度上限，非正數時不加上限制屬性</param>
        /// <returns></returns>
        public static IHtmlContent XiTextAuto(this IHtmlHelper htmlHelper, string fid, string value = "", int maxLength = 0, string hint="")
        {
            return GetStr(fid, value, maxLength, hint);
        }

        /*
        /// <summary>
        /// binding model
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="hint"></param>
        /// <param name="ftype"></param>
        /// <returns></returns>
        public static IHtmlContent XiTextAutoFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, int maxLength = 0, string hint = "")
        {
            //讀取欄位 id, value
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var fid = metadata.PropertyName;
            var value = metadata.Model == null ? "" : metadata.Model.ToString();
            return GetStr(fid, value, maxLength, hint);
        }
        */

        /// <summary>
        /// return html string for render
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="value"></param>
        /// <param name="maxLength"></param>
        /// <param name="hint"></param>
        /// <returns></returns>
        private static IHtmlContent GetStr(string fid, string value, int maxLength, string hint)
        {
            //placeholder可能會包含單引號, 所以escape處理(")
            //會顯示紅色錯誤框的element 必須在 error label 上面且相鄰  !!
            //使用bootstrap-typeahead : https://github.com/bassjobsen/Bootstrap-3-Typeahead
            var max = (maxLength > 0) ? "maxlength='" + maxLength + "'" : "";
            var html = @"
<div>
    <input data-provide='typeahead' id='{0}' name='{0}' value='{1}' type='text' class='form-control' placeholder='{2}' {5}>
</div>
<span id='{3}' class='{4}'></span>
";

            html = String.Format(html, fid, value, hint, fid + _WebFun.ErrTail, _WebFun.ErrLabCls, max);
            return new HtmlString(html);
        } 
    }
}

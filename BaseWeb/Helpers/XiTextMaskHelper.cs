using BaseWeb.Services;
using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

//todo
namespace BaseWeb.Helpers
{
    public static class XiTextMaskHelper
    {
        /// <summary>
        /// text input mask
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="fid"></param>
        /// <param name="isCheck"></param>
        /// <param name="label"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static IHtmlContent XiTextMask(this IHtmlHelper htmlHelper, string fid, string dataMask, string value="", string placeholder="")
        {
            return GetStr(fid, dataMask, value, placeholder);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="isCheck"></param>
        /// <param name="label"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        private static IHtmlContent GetStr(string fid, string dataMask, string value, string placeholder)
        {
            var html = @"<input type='text' class='form-control' id='{0}' data-mask='{2}' placeholder='{3}' value='{1}'>";
            html = String.Format(html, fid, value, dataMask, placeholder, fid + _WebFun.ErrTail, _WebFun.ErrLabCls);
            return new HtmlString(html);

        }
    }
}

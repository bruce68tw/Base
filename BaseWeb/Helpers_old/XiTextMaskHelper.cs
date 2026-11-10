using BaseWeb.Services;
using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

//todo
namespace BaseWeb.Helpers
{
    /// <summary>提供舊版遮罩文字欄位輔助方法。</summary>
    public static class XiTextMaskHelper
    {
        /// <summary>產生附有輸入遮罩設定的文字欄位。</summary>
        /// <param name="htmlHelper">目前 Razor 檢視使用的 HTML 輔助物件</param>
        /// <param name="fid">要處理的欄位識別值</param>
        /// <param name="dataMask">前端遮罩元件使用的格式字串</param>
        /// <param name="value">用於初始化欄位或選項的值</param>
        /// <param name="placeholder">輸入欄位為空時顯示的提示文字</param>
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

using BaseWeb.Services;
using BaseWeb.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BaseWeb.Helpers
{
    /// <summary>
    /// single radio, for single/multi edit form
    /// no validate span !!
    /// </summary>
    public static class XiRadioHelper
    {

        /// <summary>
        /// radio
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="fid"></param>
        /// <param name="isCheck"></param>
        /// <param name="value"></param>
        /// <param name="label"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static IHtmlContent XiRadio(this IHtmlHelper htmlHelper, string fid = "", bool isCheck = false, string value = "1", string label = "", PropInputDto prop = null)
        {
            if (prop == null)
                prop = new PropInputDto();

            //adjust
            if (label == "")
                label = "&nbsp;";   //add space, or position will wrong !!

            //attr
            //第3個參數 isInDt: false
            var attr = _Helper.GetBaseAttr(fid, prop) +
                " value='" + value + "'" +
                _Helper.GetFnOnChange("onclick", prop, "this.checked");
            if (isCheck)
                attr += " checked";

            /*
            //box class
            var boxClass = prop.Inline ? "xg-inline" : "";
            if (prop.BoxClass != "")
                boxClass += " " + prop.BoxClass;
            */

            //ext class
            //var extClass = (prop.ExtClass == "") ? "" : " " + prop.ExtClass;
            //if (prop.InDt)
            //    extClass += " xg-in-dt";

            //沒有 box container, 只有radio !!
            var html = string.Format(@"
<label class='xg-radio {0}'>
    <input type='radio'{1}>{2}
    <span></span>
</label>
", prop.ExtClass, attr, label);

            return new HtmlString(html);
        }

    }//class
}
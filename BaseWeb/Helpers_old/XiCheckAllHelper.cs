using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BaseWeb.Helpers
{
    public static class XiCheckAllHelper
    {
        /// <summary>
        /// (edit form) datatables header checkbox for checked all rows
        /// checkbox onclick fixed call _crud.onCheckAll()
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="boxStr">box container</param>
        /// <param name="dataFid">check fid(data-id value)</param>
        /// <returns></returns>
        public static IHtmlContent XiCheckAll(this IHtmlHelper htmlHelper, string boxStr, string dataFid)
        {
            //tail span is need for checkbox
            var html = string.Format(@"
<th width='60' class='text-center'>
    <label class='xg-check'>&nbsp
        <input type='checkbox' class='checkboxes' onclick='_crud.onCheckAll(this, {0}, &quot;{1}&quot;)'>
        <span></span>
    </label>
</th>
", boxStr, dataFid);

            return new HtmlString(html);
        }

    } //class
}

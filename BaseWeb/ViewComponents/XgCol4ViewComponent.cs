using Base.Services;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //add row button
    /// <summary>呈現建立人、修改人及其時間的唯讀欄位的 MVC ViewComponent。</summary>
    public class XgCol4ViewComponent : ViewComponent
    {
        /// <summary>產生建立人、修改人及其時間的唯讀欄位的 HTML。</summary>
        /// <param name="cols">傳給元件的版面欄寬設定</param>
        public HtmlString Invoke(string cols = "")
        {
			//here!!
			var colList = _Str.ToIntList(cols);
			if (colList.Count < 2)
				colList = _Fun.DefHoriColList;

            var html = string.Format(@"
<div class='row'>
	<div class='col-md-{0} x-label'>建檔人員</div>
	<div class='col-md-{1} x-input'>
		<label data-fid='CreatorName' name='CreatorName' data-type='read' class='form-control x-inline xi-read xi-unsave'></label>
	</div>
	<div class='col-md-{0} x-label'>修改人員</div>
	<div class='col-md-{1} x-input'>
		<label data-fid='ReviserName' name='ReviserName' data-type='read' class='form-control x-inline xi-read xi-unsave'></label>
	</div>
</div>
<div class='row'>
    <div class='col-md-{0} x-label'>建檔時間</div>
    <div class='col-md-{1} x-input'>
        <label data-fid='Created' name='Created' data-format='MmUiDtFmt' data-type='read' class='form-control x-inline xi-read xi-unsave'></label>
    </div>
    <div class='col-md-{0} x-label'>修改日期</div>
    <div class='col-md-{1} x-input'>
        <label data-fid='Revised' name='Revised' data-format='MmUiDtFmt' data-type='read' class='form-control x-inline xi-read xi-unsave'></label>
    </div>
</div>
", colList[0], colList[1]);

            return new HtmlString(html);
        }
    }//class
}

using Base.Services;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //add row button
    public class XgCol4ViewComponent : ViewComponent
    {
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

using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    public class XgToolViewComponent : ViewComponent
    {
        /// <summary>
        /// include: msg, ans, alert, wait
        /// css use id, not class !!
        /// </summary>
        /// <returns>must be HtmlString, or will get extra quote !!</returns>
        public HtmlString Invoke()
        {
            //modal html
            var baseR = _Locale.GetBaseRes();
/*
<!-- wait -->
<div id='xgWait'>
    <i class='fa fa-spinner fa-spin fa-3x fa-fw'></i>
</div> 
*/
            var html = $@"
<!-- alert -->
<div id='xgAlert' class='alert alert-success alert-dismissable' style='display:none;'>
    <a href='#' class='close' onclick='_tool.onAlertClose();'>&times;</a>
    <span class='xd-msg'></span>
</div>

<!-- msg -->
<div id='xgMsg' class='modal fade xg-msg'>
    <div class='modal-dialog' role='document'>
        <div class='modal-content'>
            <div class='modal-header'>
                <button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close' style='padding:10px 10px;'></button>
            </div>
            <div class='modal-body'>
                <p class='xd-msg'></p>
            </div>
            <div class='modal-footer'>
                <button type='button' class='btn btn-primary xd-close' onclick='_tool.onMsgClose()'>{baseR.BtnClose}</button>
            </div>
        </div>
    </div>
</div>

<!-- ans -->
<div id='xgAns' class='modal fade xg-msg'>
    <div class='modal-dialog' role='document'>
        <div class='modal-content'>
            <div class='modal-header'>
                <button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close' style='padding:10px 10px;'></button>
            </div>
            <div class='modal-body'>
                <p class='xd-msg'></p>
            </div>
            <div class='modal-footer'>
                <button type='button' class='btn btn-secondary xg-btn-size xd-cancel' onclick='_tool.onAnsNo()'>{baseR.BtnCancel}</button>
                <button type='button' class='btn btn-primary xg-btn-size xd-yes' onclick='_tool.onAnsYes()'>{baseR.BtnYes}</button>
            </div>
        </div>
    </div>
</div>

<!-- textarea(many lines) editor -->
<div id='xgArea' class='modal fade xg-modal' data-backdrop='static' data-keyboard='false'>
    <div class='modal-dialog' role='document'>
        <div class='modal-content'>
            <div class='modal-header'>
                <h4 class='modal-title'></h4>
                <button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close' style='padding:10px 10px;'></button>
            </div>
            <div class='modal-body'>
                <textarea value='' style='width:100%' rows='8' maxlength='1000' class='form-control' aria-invalid='false'>
                </textarea>
            </div>
            <div class='modal-footer'>
                <button type='button' class='btn btn-secondary xg-btn-size xd-cancel' data-bs-dismiss='modal'>{baseR.BtnCancel}</button>
                <button type='button' class='btn btn-primary xg-btn-size xd-yes' onclick='_tool.onAreaYes()'>{baseR.BtnYes}</button>
            </div>
        </div>
    </div>
</div>

<!-- show image -->
<div id='xgImage' class='modal fade'>
    <div class='modal-dialog' role='document'>
        <div class='modal-content'>
            <div class='modal-body' style='margin:auto;'>
				<img/>
            </div>
            <div class='modal-footer justify-content-between'>
				<label>image name</label>
                <button type='button' class='btn btn-primary xg-btn-size xd-close' data-bs-dismiss='modal'>{baseR.BtnClose}</button>
            </div>
        </div>
    </div>
</div>
";
            return new HtmlString(html);
        }

    }//class
}

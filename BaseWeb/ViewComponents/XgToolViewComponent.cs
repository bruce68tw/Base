using BaseApi.Services;
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
<div class='x-wait'>
    <i class='fa fa-spinner fa-spin fa-3x fa-fw'></i>
</div> 
*/
            var html = $@"
<!-- alert -->
<div class='alert alert-success alert-dismissable x-alert d-none'>
    <span class='xd-msg'></span>
    <button type='button' class='btn btn-link' onclick='_tool.onAlertClose();'>
        <i class='ico-delete'></i>
    </button>
</div>

<!-- msg/ans class 都有 x-msg for style, 使用id建立jQuery object -->
<!-- msg -->
<div id='xgMsg' class='modal fade x-msg'>
    <div class='modal-dialog' role='document'>
        <div class='modal-content'>
            <div class='modal-header'>
                <button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button>
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
<div id='xgAns' class='modal fade x-msg'>
    <div class='modal-dialog' role='document'>
        <div class='modal-content'>
            <div class='modal-header'>
                <button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button>
            </div>
            <div class='modal-body'>
                <p class='xd-msg'></p>
            </div>
            <div class='modal-footer'>
                <button type='button' class='btn btn-secondary xd-cancel' onclick='_tool.onAnsNo()'>{baseR.BtnCancel}</button>
                <button type='button' class='btn btn-primary xd-yes' onclick='_tool.onAnsYes()'>{baseR.BtnYes}</button>
            </div>
        </div>
    </div>
</div>

<!-- textarea(many lines) editor -->
<div class='modal fade x-modal x-area' data-backdrop='static' data-keyboard='false'>
    <div class='modal-dialog' role='document'>
        <div class='modal-content'>
            <div class='modal-header'>
                <h4 class='modal-title'></h4>
                <button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button>
            </div>
            <div class='modal-body'>
                <textarea value='' rows='8' maxlength='1000' class='form-control' aria-invalid='false'>
                </textarea>
            </div>
            <div class='modal-footer'>
                <button type='button' class='btn btn-secondary xd-cancel' data-bs-dismiss='modal'>{baseR.BtnCancel}</button>
                <button type='button' class='btn btn-primary xd-yes' onclick='_tool.onAreaYes()'>{baseR.BtnYes}</button>
            </div>
        </div>
    </div>
</div>

<!-- show image -->
<div class='modal fade x-image'>
    <div class='modal-dialog modal-dialog-centered'>
        <div class='modal-content'>
            <div class='modal-body'>
                <img/>
            </div>
            <div class='modal-footer justify-content-between'>
                <label>image name</label>
                <button class='btn btn-primary xd-close' data-bs-dismiss='modal'>{baseR.BtnClose}</button>
            </div>
        </div>
    </div>
</div>
";
            return new HtmlString(html);
        }

    }//class
}

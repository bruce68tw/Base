using BaseApi.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //草稿、回上一頁、送出
    /// <summary>呈現返回、儲存草稿與送出按鈕組的 MVC ViewComponent。</summary>
    public class XgSave3ViewComponent : ViewComponent
    {
        /// <summary>產生返回、儲存草稿與送出按鈕組的 HTML。</summary>
        /// <param name="align">按鈕區塊的水平對齊設定</param>
        /// <param name="fnOnBack">按下返回按鈕時呼叫的前端函式</param>
        /// <param name="fnOnDraft">按下儲存草稿按鈕時呼叫的前端函式</param>
        /// <param name="fnOnSave">按下儲存按鈕時呼叫的前端函式</param>
        public HtmlString Invoke(string align = "center", string fnOnBack = "_me.crudR.onToRead",
            string fnOnDraft = "_me.crudE.onDraft",
            string fnOnSave = "_me.crudE.onSave")
        {
            var baseR = _Locale.GetBaseRes();
            var html = $@"
<div class='x-btns-box x-{align}'>
    <button id='btnToRead' type='button' class='btn x-btn-cancel' data-onclick='{fnOnBack}'>{baseR.BtnToRead}<i class='ico-back'></i></button>
    <button id='btnDraft' type='button' class='btn x-btn2' data-edit data-onclick='{fnOnDraft}'>{baseR.BtnDraft}<i class='ico-save'></i></button>
    <button id='btnSave' type='button' class='btn x-btn1' data-edit data-onclick='{fnOnSave}'>{baseR.BtnSaveSend}<i class='ico-checked'></i></button>
</div>
";
            return new HtmlString(html);
        }

    } //class
}
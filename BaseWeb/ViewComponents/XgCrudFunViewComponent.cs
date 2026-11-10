using BaseApi.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //改成前端設定??
    /// <summary>呈現單列資料的修改、刪除與檢視按鈕的 MVC ViewComponent。</summary>
    public class XgCrudFunViewComponent : ViewComponent
    {
        /// <summary>產生單列資料的修改、刪除與檢視按鈕的 HTML。</summary>
        /// <param name="key">用來定位資料的鍵值</param>
        /// <param name="rowName">隨刪除事件一併傳入的資料列顯示名稱</param>
        /// <param name="hasUpdate">是否顯示修改操作</param>
        /// <param name="hasDelete">是否顯示刪除操作</param>
        /// <param name="hasView">是否顯示檢視操作</param>
        /// <param name="fnOnUpdate">修改操作的前端函式，空字串時使用預設函式</param>
        /// <param name="fnOnDelete">刪除操作的前端函式，空字串時使用預設函式</param>
        /// <param name="fnOnView">檢視操作的前端函式，空字串時使用預設函式</param>
        public HtmlString Invoke(string key, string rowName = "", 
            bool hasUpdate = false, bool hasDelete = false, bool hasView = false,
            string fnOnUpdate = "", string fnOnDelete = "", string fnOnView = "")
        {
            var br = _Locale.GetBaseRes();
            var funs = "";
            if (hasUpdate)
                funs += $"<button type='button' class='btn btn-link' data-onclick='{(fnOnUpdate == "" ? "_me.crudR.onUpdate" : fnOnUpdate)}' data-args='{key}'><i class='ico-pen' title='{br.TipUpdate}'></i></button>";
            if (hasDelete)
                funs += $"<button type='button' class='btn btn-link' data-onclick='{(fnOnDelete == "" ? "_me.crudR.onDelete" : fnOnDelete)}' data-args='{key},{rowName}'><i class='ico-delete x-delete' title='{br.TipDelete}'></i></button>";
            if (hasView)
                funs += $"<button type='button' class='btn btn-link' data-onclick='{(fnOnView == "" ? "_me.crudR.onView" : fnOnView)}' data-args='{key}'><i class='ico-eye' title='{br.TipView}'></i></button>";
            return new HtmlString(funs);
        }
    }
}
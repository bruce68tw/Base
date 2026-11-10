using BaseApi.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>呈現新增作業按鈕的 MVC ViewComponent。</summary>
    public class XgCreateViewComponent : ViewComponent
    {
        /// <summary>產生新增作業按鈕的 HTML。</summary>
        /// <param name="fnOnClick">按鈕點擊時要呼叫的前端函式</param>
        public HtmlString Invoke(string fnOnClick = "_me.crudR.onCreate")
        {
            var label = _Locale.GetBaseRes().BtnCreate;
            //label和icon之間不要斷行, 否則會產生空格
            return new HtmlString($@"
<button type='button' class='btn x-btn2 xd-create' data-onclick='{fnOnClick}'>{label}<i class='ico-plus'></i>
</button>");
        }
    }
}
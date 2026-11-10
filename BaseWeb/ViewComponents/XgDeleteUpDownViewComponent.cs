using Base.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //多筆資料的新增,刪除按鈕
    /// <summary>呈現資料列刪除與上移、下移按鈕的 MVC ViewComponent。</summary>
    public class XgDeleteUpDownViewComponent : ViewComponent
    {
        //use button for control status
        //mName: EditMany.js variables name
        /// <summary>產生資料列刪除與上移、下移按鈕的 HTML。</summary>
        /// <param name="mName">用來組合預設刪除列函式的前端物件名稱</param>
        /// <param name="fnDeleteRow">刪除列的前端函式，空字串時依物件名稱組合</param>
        public HtmlString Invoke(string mName, string fnDeleteRow = "")
        {
            if (_Str.IsEmpty(fnDeleteRow))
                fnDeleteRow = $"{mName}.onDeleteRow";

            //加上 xd-delete 顯示紅色
            var html = $@"
<button type='button' class='btn btn-link' data-edit data-onclick='{fnDeleteRow}'>
    <i class='ico-delete x-delete'></i>
</button>
<button type='button' class='btn btn-link' data-edit data-onclick='_table.rowMoveUp'>
    <i class='ico-up'></i>
</button>
<button type='button' class='btn btn-link' data-edit data-onclick='_table.rowMoveDown'>
    <i class='ico-down'></i>
</button>";
            return new HtmlString(html);
        }        

    }//class
}

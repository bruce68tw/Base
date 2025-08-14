using Base.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //多筆資料的新增,刪除按鈕
    public class XgDeleteUpDownViewComponent : ViewComponent
    {
        //use button for control status
        //mName: EditMany.js variables name
        public HtmlString Invoke(string mName, string fnDeleteRow = "")
        {
            if (_Str.IsEmpty(fnDeleteRow))
                fnDeleteRow = $"{mName}.onDeleteRow";

            var html = $@"
<button type='button' class='btn btn-link' data-onclick='{fnDeleteRow}'>
    <i class='ico-delete'></i>
</button>
<button type='button' class='btn btn-link' data-onclick='_table.rowMoveUp'>
    <i class='ico-up'></i>
</button>
<button type='button' class='btn btn-link' data-onclick='_table.rowMoveDown'>
    <i class='ico-down'></i>
</button>";
            return new HtmlString(html);
        }        

    }//class
}

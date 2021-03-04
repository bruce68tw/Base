//todo
namespace BaseWeb.Helpers
{
    /// <summary>
    /// todo: 再檢查
    /// 單選下拉式欄位, 每個選項有header
    /// </summary>
    public static class XiSelectExtHelper
    {
        /*
        /// <summary>
        /// 下拉式欄位, 使用 bootstrap-select
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="rows">data source, 陣列內容依次為: title, key, value</param>
        /// <param name="showValue">true(顯示value), false(顯示title)</param>
        /// <returns></returns>
        public static IHtmlContent XiSelectExt(this IHtmlHelper htmlHelper, string fid, string value, List<IdStrExtModel> rows, bool showValue = true, PropSelectDto prop = null)
        {
            return GetStr(fid, value, rows, showValue, prop);
        }

        public static IHtmlContent XiSelectExtFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, List<IdStrExtModel> rows, bool showValue = true, PropSelectDto prop = null)
        {
            //讀取欄位 id
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var fid = metadata.PropertyName;
            var value = metadata.Model != null ? metadata.Model.ToString() : "";
            return GetStr(fid, value, rows, showValue, prop);
        }

        private static IHtmlContent GetStr(string fid, string value, List<IdStrExtModel> rows, bool showValue = true, PropSelectDto prop = null)
        {
            //var htmlRow = "";
            var list = "";
            var selected = "";
            
            //var values = _Str.ToList(value, (char)seperator);
            var htmlRow = @"
<option {4} style='padding-left:10px;' value='{0}' data-content=""
    <div style='font-weight:bold;'>{2}</div>
    <div style='white-space:pre-wrap;'>{1}</div>
"">{3}</option>
";

           //加上第一筆空白選項的資料, 設定空白列title='' 才會顯示 placeHolder內容 !!
           if (prop == null || prop.AddEmptyRow)
               list += String.Format("<option value='{0}' {2}>{1}</option>", "", _Fun.SelectText, "title=''");

            for (var i = 0; i < rows.Count; i++)
            {
                //List<string> items = rows[i];
                selected = (value == rows[i].Id) ? "selected" : "";
                var text = showValue ? rows[i].Str : rows[i].Ext;
                list += String.Format(htmlRow, rows[i].Id, rows[i].Str, rows[i].Ext, text, selected);
            }

            //寬度必須為 data-width='100%' 才會有 RWD效果 !!
            //用class來控制多欄位 !!
            //xg-select-col 用來設定dropdown內框width=100%, xg-select-colX 用來設定RWD寬度
            //會顯示紅色錯誤框的element 必須在 error label 上面且相鄰  !!
            var html = @"
<div style='width:100%'>
    <select id='{0}' name='{0}' data-show-content='false' {4}>
        {1}
    </select>
    <span id='{2}' class='{3}'></span>
</div>
";
            //get select property string
            var prop2 = _Helper.GetSelectProp(prop);

            html = String.Format(html, fid, list, fid + _WebFun.ErrTail, _WebFun.ErrLabCls, prop2);
            return new HtmlString(html);
        } 
        */

    }//class
}

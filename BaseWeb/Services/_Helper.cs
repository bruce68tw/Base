using Base.Models;
using Base.Services;
using System.Collections.Generic;

namespace BaseWeb.Services
{

    public static class _Helper
    {
        //public const string XgRequired = "xg-required";     //for label
        public const string XdRequired = "required";     //for input ??

        /*
        //for helper binding
        public static void GetMetaValue<TParameter, TValue>(out string fid, out string value, Expression<Func<TParameter, TValue>> expression, ViewDataDictionary<TParameter> viewData)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, viewData);
            fid = metadata.PropertyName;
            value = metadata.Model == null ? "" : metadata.Model.ToString();
            //return metadata.Model != null ? metadata.Model.ToString() : "";
        }
        */

        /// <summary>
        /// get label html string with required sign.
        /// </summary>
        /// <param name="required"></param>
        /// <returns></returns>
        public static string GetRequiredSpan(bool required)
        {
            return required ? "<span class='xg-required'>*</span>" : "";
        }

        /// <summary>
        /// get label tip with icon
        /// </summary>        
        public static string GetIconTip()
        {
            return "<i class='ico-info'></i>";
        }

        #region get attribute
        /// <summary>
        /// get input attr: data-fid,name,readonly, ext attributes
        /// </summary>
        /// <param name="fid">if empty will not set name attribute</param>
        /// <param name="edit"></param>
        /// <param name="inputAttr"></param>
        /// <param name="setName">set name attribute or not</param>
        /// <returns></returns>
        public static string GetInputAttr(string fid,
            string edit = "", bool required = false, string inputAttr = "")
        {
            //set data-fid, name
            var attr = _Str.IsEmpty(fid)
                ? " " : $" data-fid='{fid}' name='{fid}'";
            //if (!edit)
            //    attr += " readonly";
            attr += " " + GetDataEdit(edit);
            if (required)
                attr += " required";
            if (inputAttr != "")
                attr += " " + inputAttr;
            return attr;
        }

        //get data-edit attribute string
        public static string GetDataEdit(string edit)
        {
            if (_Str.IsEmpty(edit))
                edit = "*";
            return $" data-edit='{edit}'";
        }

        //add placeholder attribute
        //placeholder could have quota, use escape
        public static string GetPlaceHolder(string inputTip)
        {
            return (inputTip == "")
                ? ""
                : " placeholder='" + inputTip + "'";
        }

        //get required attribute
        public static string GetRequired(bool required)
        {
            return required ? " required" : "";
        }

        //get maxlength attribute
        public static string GetMaxLength(int maxLen)
        {
            return (maxLen > 0) 
                ? " maxlength='" + maxLen + "'" 
                : "";
        }

        /*
        //return ext class
        public static string GetClass(string extClass)
        {
            return " class='form-control" +
                (extClass == "" ? "'" : " " + extClass + "'");
        }

        //set prop.FnOnChange
        public static string GetFnOnChange(string fnName, PropBaseDto prop, string arg)
        {
            return (prop.FnOnChange == "")
                ? ""
                : " " + fnName + "='" + (prop.FnOnChange.IndexOf("(") > 0 ? prop.FnOnChange : prop.FnOnChange + "(" + arg + ")") + "'";
        }
        */
        #endregion

        #region get html string
        /// <summary>
        /// get date view component html string
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="value">format: _Fun.CsDtFormat</param>
        /// <param name="type">data-type, empty for part field, ex:DateTime</param>
        /// <param name="required"></param>
        /// <returns></returns>
        public static string GetDateHtml(string fid, string value, string type, 
            bool required = false, string edit = "", string inputTip = "",             
            string inputAttr = "", string boxClass = "")
        {
            //input field attribute
            string attr;
            if (_Str.IsEmpty(type))
            {
                //no fid, name
                attr = GetInputAttr("", edit, required);
            }
            else
            {
                //only fid
                attr = GetInputAttr(fid, edit, required) +
                    $" data-type='{type}'";
                boxClass += " xi-box";
            }
            attr += GetPlaceHolder(inputTip);

            //value -> date format
            value = _Date.GetDateStr(value);
            var dataEdit = GetDataEdit(edit);

            //input-group & input-group-addon are need for datepicker !!
            return $@"
<div class='input-group date {boxClass}' data-provide='datepicker' {inputAttr}>
    <input{attr} value='{value}' type='text' class='form-control'>
    <div class='input-group-addon'></div>
    <span>
        <i class='ico-delete' onclick='_idate.onReset(this)'></i>
        <i class='ico-date' onclick='_idate.onToggle(this)'></i>
    </span>
</div>";
        }

        //get link function string
        public static string GetLinkFn(string fn)
        {
            return $"event.preventDefault();{fn};return false;";
        }

        /// <summary>
        /// get select view component html string
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="value"></param>
        /// <param name="type">empty means part component</param>
        /// <param name="rows"></param>
        /// <param name="required"></param>
        /// <param name="edit"></param>
        /// <param name="addEmptyRow"></param>
        /// <param name="inputTip"></param>
        /// <param name="inputAttr"></param>
        /// <param name="boxClass"></param>
        /// <param name="fnOnChange"></param>
        /// <returns></returns>
        public static string GetSelectHtml(string fid, string value, 
            string type, List<IdStrDto> rows,
            bool required = false, string edit = "", bool addEmptyRow = true, 
            string inputTip = "", string inputAttr = "", string boxClass = "",
            string fnOnChange = "")
        {
            var hasType = !_Str.IsEmpty(type);
            string attr = hasType
                ? GetInputAttr(fid, edit, required, inputAttr) + $" data-type='{type}'"
                : GetInputAttr("", edit, required, inputAttr);
            attr += GetPlaceHolder(inputTip);
            if (!_Str.IsEmpty(fnOnChange))
                attr += $" onchange='{fnOnChange}'";

            //ext class
            //var extClass = required ? XdRequired : "";
            if (hasType)
                boxClass += " xi-box";

            //option item
            var optList = "";
            var tplOpt = "<option value='{0}'{2}>{1}</option>";

            //add first empty row & set its title='' to show placeHolder !!
            if (addEmptyRow)
                optList += string.Format(tplOpt, "", _Locale.GetBaseRes().PlsSelect, "title=''");

            var len = (rows == null) ? 0 : rows.Count;
            for (var i = 0; i < len; i++)
            {
                var selected = (value == rows[i].Id) ? " selected" : "";
                optList += string.Format(tplOpt, rows[i].Id, rows[i].Str, selected);
            }

            //set data-width='100%' for RWD !!
            //use class for multi columns !!
            //xg-select-col for dropdown inner width=100%, xg-select-colX for RWD width
            return $@"
<select{attr} class='form-control {boxClass}'>
    {optList}
</select>";            
        }

        /*
        public static string GetTextareaHtml(string title, string fid, 
            string type, string value = "",
            int maxLen = 0, int rowsCount = 3,
            bool required = false, bool editable = true, bool inRow = false,
            string labelTip = "", string inputTip = "",            
            string extAttr = "", string extClass = "", string cols = "")
        {
            //attr
            var attr = _Helper.GetInputAttr(fid, editable, required) +
                $" value='{value}' rows='{rowsCount}' style='width:100%'" +
                GetPlaceHolder(inputTip) +
                GetRequired(required) +
                GetMaxLength(maxLen);
            if (!_Str.IsEmpty(extAttr))
                attr += " " + extAttr;

            //html
            var html = $"<textarea{attr} data-type='{type}' class='form-control xi-box {extClass}'></textarea>";
            if (!_Str.IsEmpty(title))
                html = InputAddLayout(html, title, required, labelTip, inRow, cols);

            //html = String.Format(html, attr, _Html.Decode(value), extClass, fid + _WebFun.Error, _WebFun.ErrorLabelClass);
            return html;
        }
        */
        #endregion

        /// <summary>
        /// get input field html
        /// </summary>
        /// <param name="html"></param>
        /// <param name="title"></param>
        /// <param name="required"></param>
        /// <param name="labelTip"></param>
        /// <param name="cols">ary0(是否含 row div), ary1,2(for 水平), ary1(for 垂直)</param>
        /// <param name="labelHideRwd">RWD(phone) hide label</param>
        /// <returns></returns>
        public static string InputAddLayout(string html, string title, bool required, 
            string labelTip, bool inRow, string cols, bool labelHideRwd = false)
        {
            //cols = cols ?? _Fun.DefHCols;
            var colList = GetCols(cols);
            var labelTip2 = "";
            var iconTip = "";
            if (!_Str.IsEmpty(labelTip))
            {
                labelTip2 = " title='" + labelTip + "'";
                iconTip = GetIconTip();
            }
            var reqSpan = GetRequiredSpan(required);
            string result;
            var labelClass = "xg-label";
            if (labelHideRwd)
                labelClass += " " + _Fun.HideRwd;
            if (colList.Count > 1)
            {
                //horizontal
                result = string.Format(@"
<div class='col-md-{0} {5}'{2}>{3}</div>
<div class='col-md-{1} xg-input'>
    {4}
</div>
", colList[0], colList[1], labelTip2, (reqSpan + title + iconTip), html, labelClass);
            }
            else
            {
                //vertical
                result = string.Format(@"
<div class='col-md-{0} xg-row'>
    <div class='{4}'{1}>{2}</div>
    <div class='xg-input'>
        {3}
    </div>
</div>
", colList[0], labelTip2, (reqSpan + title + iconTip), html, labelClass);
            }

            //if not in row, add row container
            if (!inRow)
                result = "<div class='row'>" + result + "</div>";
            return result;
        }

        private static List<int> GetCols(string cols)
        {
            var values = _Str.ToIntList(cols);
            return (values == null) ? _Fun.DefHoriCols : values;
            /*
            var len = values.Count;
            if (len == 1)
                values.Add(values[0]);
            return values;
            */
        }

        /*
        /// <summary>
        /// ??
        /// 傳回下拉式欄位屬性字串 (bootstrap-select)
        /// </summary>
        /// <param name="prop">select 屬性 model</param>
        /// <param name="title">select title, 如果空白, 則會使用 prop.PlaceHolder</param>
        /// <returns>屬性字串 for select tag</returns>
        //public static string GetSelectProp(SelectPropModel prop, string title = "")
        public static string GetSelectProp(Select2PropModel prop)
        {
            //initial result
            //寬度必須為 data-width='100%' 才會有 RWD效果 !!
            //20161201 增加 disableslimscroll class, 手機 scrolling 才會正常 !!
            //20161215 Bruce : disableslimscroll 移到 _fun.js 處理
            var result = "data-style='" + (prop!=null?prop.ButtonClass:"") + " xg-select-btn' data-width='100%'";
            var className = "selectpicker";     //bootstrap-select default class

            //change title if need, 必須使用空白字元字碼, 直接使用空白字元無作用 !!
            //var title = (prop == null || prop.PlaceHolder == "") ? "&nbsp;" : prop.PlaceHolder;
            //20161205 Bruce : 必須將單引號轉換成為 &#39; 才能在前端正確顯示 !!
            var title = (prop == null || String.IsNullOrEmpty(prop.PlaceHolder)) ? "&nbsp;" : prop.PlaceHolder.Replace("'", "&#39;");

            result += " title='" + title + "'";

            if (prop != null)
            {

                //顯示筆數, for 考慮下拉式欄位空間
                if (prop.Size > 0)
                    result += " data-size='" + prop.Size + "'";

                //change className
                //xg-select-col 用來設定dropdown內框width=100%, xg-select-colX 用來設定RWD寬度
                if (prop.Columns > 1)
                    className += " xg-select-col xg-select-col" + prop.Columns;
                if (prop.ClassName != "")
                    className += " " + prop.ClassName;

                //add onchange
                if (!_Str.IsEmpty(prop.OnChange))
                    result += " onchange='" + prop.OnChange + "(this)'";

                //data-separator 用來傳遞自訂參數
                result += " data-separator='" + prop.Separator + "'";

                //dragupauto
                if (!prop.DropUpAuto)
                    result += " data-dropup-auto='false'";
            }
            result += " class='" + className + "'";
            return result;
        }
        */

    }//class
}

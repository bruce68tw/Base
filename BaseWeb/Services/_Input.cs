using Base.Enums;
using Base.Models;
using Base.Services;
using BaseApi.Services;
using BaseWeb.Models;
using System.Collections.Generic;
using System.Linq;

namespace BaseWeb.Services
{

    //_Helper -> _Input
    public static class _Input
    {
        //public const string XgRequired = "x-required";     //for label
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
            return required ? "<span class='x-required'>*</span>" : "";
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

        //get required attribute ??
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

        public static string GetPattern(string pattern)
        {
            return (string.IsNullOrEmpty(pattern) || pattern == InputPatternEstr.None)
                ? ""
                : " pattern='" + pattern + "'";
        }

        /// <summary>
        /// return ext class for 輸入欄位
        /// </summary>
        /// <param name="clsNow"></param>
        /// <param name="clsExt"></param>
        /// <param name="width">如果有值則會使用 x-inline 並且使用固定寬度 x-wxxx</param>
        /// <returns></returns>
        public static string GetCssClass(string clsNow, string clsExt, int width)
        {
            if (clsExt != "")
                clsNow += " " + clsExt;
            if (width > 0)
                clsNow += $" x-inline x-w{width}";
            return clsNow;
        }

        /// <summary>
        /// get event attribute string for onclick, onchange, etc.
        /// </summary>
        /// <param name="fnName">html event ex: onclick, onchange</param>
        /// <param name="fnValue"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string GetEventAttr(string fnName, string fnValue, string args = "")
        {
            if (string.IsNullOrEmpty(fnValue))
                return "";

            var attr = $"data-{fnName}='{fnValue}'";
            if (!string.IsNullOrEmpty(args))
                attr += $" data-args='{args}'";
            return attr;
        }
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
            string inputAttr = "", string clsBox = "")
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
                attr = GetInputAttr(fid, edit, required) + $" data-type='{type}'";
                clsBox += " xi-box";
            }
            attr += GetPlaceHolder(inputTip);

            //value -> date format
            value = _Date.GetDateStr(value);
            //var dataEdit = GetDataEdit(edit);

            //xidate 無條件加上 x-inline, 同時 .date會設定width=180px
            //使用 .date 執行 _idate 初始化, 因為包含多個元素, 所以必須將box對應datepicker !!
            //input-group & input-group-addon are need for datepicker !!
            return $@"
<div class='input-group date x-inline {clsBox}' data-provide='datepicker' {inputAttr}>
    <input{attr} value='{value}' type='text' class='form-control'>
    <div class='input-group-addon'></div>
    <span>
        <i class='ico-delete' data-onclick='_idate.onReset'></i>
        <i class='ico-date' data-onclick='_idate.onToggle'></i>
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
        /// <param name="clsBox"></param>
        /// <param name="fnOnChange"></param>
        /// <returns></returns>
        public static string GetSelectHtml(string fid, string value, 
            string type, List<IdStrDto> rows,
            bool required = false, string edit = "", bool addEmptyRow = true, 
            string inputTip = "", string inputAttr = "", string clsBox = "",
            string fnOnChange = "", string eventArgs = "")
        {
            var hasType = _Str.NotEmpty(type);
            string attr = hasType
                ? GetInputAttr(fid, edit, required, inputAttr) + $" data-type='{type}'"
                : GetInputAttr("", edit, required, inputAttr);
            attr += GetPlaceHolder(inputTip);
            if (_Str.NotEmpty(fnOnChange))
                attr += " " + GetEventAttr("onchange", fnOnChange, eventArgs);

            //ext class
            if (hasType)
                clsBox += " xi-box";

            //option item
            var optList = "";
            var tplOpt = "<option value='{0}'{2}>{1}</option>";

            //add first empty row & set its title='' to show placeHolder !!
            if (addEmptyRow)
                optList += string.Format(tplOpt, "", _Locale.GetBaseRes().PlsSelect, "title=''");

            var len = (rows == null) ? 0 : rows.Count;
            for (var i = 0; i < len; i++)
            {
                var selected = (value == rows![i].Id) ? " selected" : "";
                optList += string.Format(tplOpt, rows[i].Id, rows[i].Str, selected);
            }

            //set data-width='100%' for RWD !!
            //use class for multi columns !!
            //x-select-col for dropdown inner width=100%, x-select-colX for RWD width
            return $@"
<select{attr} class='form-select {clsBox}'>
    {optList}
</select>";            
        }

        #endregion

        /// <summary>
        /// get input field html
        /// </summary>
        /// <param name="html"></param>
        /// <param name="title"></param>
        /// <param name="required"></param>
        /// <param name="labelTip"></param>
        /// <param name="inRow">如果false則會在外面包一層row class</param>
        /// <param name="cols">ary0(是否含 row div), ary1,2(for 水平), ary1(for 垂直)</param>
        /// <param name="labelHideRwd">RWD(phone) hide label</param>
        /// <returns></returns>
        //private static string InputAddLayout(string html, string title, bool required, 
        //    string labelTip, bool inRow, string cols, bool labelHideRwd = false)
        private static string InputAddLayout(string html, bool required, XiBaseDto dto, bool labelHideRwd = false)
        {
            //加上 label tip(title)
            //cols = cols ?? _Fun.DefHCols;
            var colList = GetCols(dto.Cols);
            var labelTip2 = "";
            var iconTip = "";
            if (_Str.NotEmpty(dto.LabelTip))
            {
                labelTip2 = " title='" + dto.LabelTip + "'";
                iconTip = GetIconTip();
            }

            //加上 required, label內容順序固定為 req、title、tip(前端按照此順序處理!!)
            var reqSpan = GetRequiredSpan(required);
            var clsLabel = labelHideRwd ? _Fun.ClsHideRwd : "";
            string result;
            if (colList.Count > 1)
            {
                //horizontal
                //加上 input tail for 水平label,input only
                var inputTail = "";
                if (!string.IsNullOrEmpty(dto.InputNote))
                {
                    var colSum = colList.Sum(a => a);
                    var col2 = (colSum < 6 ? 6 : 12) - colSum;
                    inputTail = $"<div class='col-md-{col2} x-input-note'>{dto.InputNote}</div>";
                }

                //get html
                clsLabel += " x-label";
                result = string.Format(@"
<div class='col-md-{0} {5}'{2}>{3}</div>
<div class='col-md-{1} x-input'>
    {4}
</div>
{6}
", colList[0], colList[1], labelTip2, (reqSpan + dto.Title + iconTip), html, _Str.KeepOneSpace(clsLabel), inputTail);
            }
            else
            {
                //vertical
                clsLabel += " x-vlabel";
                result = string.Format(@"
<div class='col-md-{0} zz_x-row'>
    <div class='{4}'{1}>{2}</div>
    <div class='x-input'>
        {3}
    </div>
</div>
", colList[0], labelTip2, (reqSpan + dto.Title + iconTip), html, _Str.KeepOneSpace(clsLabel));
            }

            //if not in row, add row container
            if (!dto.InRow)
                result = "<div class='row'>" + result + "</div>";
            return result;
        }

        private static List<int> GetCols(string? cols)
        {
            var values = _Str.ToIntList(cols);
            return (values.Count == 0) ? _Fun.DefHoriColList : values;
        }

        #region 傳回各種輸入欄位 html 字串, 其他程式會用到
        public static string XiCheck(XiCheckDto dto)
        {
            //prop ??= new PropCheckDto();
            //if (label == "")
            //    label = "&nbsp;";   //add space, or position will wrong

            //set default 
            dto.Value = _Str.EmptyToValue(dto.Value, "1");

            //get attr
            var attr = GetInputAttr(dto.Fid, dto.Edit, false, dto.InputAttr);
            if (dto.IsCheck)
                attr += " checked";
            if (_Str.NotEmpty(dto.FnOnClick))
                attr += $" data-onclick='{dto.FnOnClick}'";

            //ext class
            if (string.IsNullOrEmpty(dto.Label))
                dto.ClsBox += " x-no-label";

            //get html (span for checkbox checked sign)
            //value attr will disappear, use data-value instead !!
            var css = GetCssClass("xi-check", dto.ClsBox, dto.Width);
            var html = $@"
<label class='{css}'>
    <input{attr} type='checkbox' data-type='{InputTypeEstr.Check}' data-value='{dto.Value}'>{dto.Label}
    <span class='xi-cspan'></span>
</label>";

            //add title
            if (_Str.NotEmpty(dto.Title))
                html = InputAddLayout(html, false, dto, true);
            return html;
        }
        public static string XiDate(XiDateDto dto)
        {
            var html = GetDateHtml(dto.Fid, dto.Value, InputTypeEstr.Date, dto.Required,
                dto.Edit, dto.InputTip, dto.InputAttr, dto.ClsBox);
            if (_Str.NotEmpty(dto.Title))
                html = InputAddLayout(html, dto.Required, dto);
            return html;

            /* 
            //使用 bootstrap-datepicker-mobile 無法寫入初始值, 先不開啟此功能 !!
            if (_Device.IsMobile())
            {
                html = @"
<input type='text' class='date-picker form-control' id='{0}' name='{0}' value='{1}' placeholder=""{2}""
    data-date-format='{5}' data-date='{1}' />
<span id='{3}' class='{4}'></span>
";

            }
            else
            {

            html = @"
<div class='input-group date datepicker' data-date-format='{5}'>
    <input type='text' id='{0}' name='{0}' value='{1}' class='form-control' placeholder=""{2}"">
    <div class='input-group-addon'>
        <i class='fa fa-calendar' aria-hidden='true'></i>
    </div>
</div>
<span id='{3}' class='{4}'></span>
";
            //}
            //暫解
            if (value.Length > 10)
                value = value.Substring(0, 10).TrimEnd();

            //把日期轉換成為所需要的格式
            html = String.Format(html, fid, value, placeHolder, fid + _WebFun.Error, _WebFun.ErrorLabelClass, _Locale.DateFormatFront);
            return new HtmlString(html);
            */
        }
        public static string XiDec(XiDecDto dto)
        {
            //attr, both digits/number should be type=number for validate(digits not work !!)
            var attr = GetInputAttr(dto.Fid, dto.Edit, dto.Required, dto.InputAttr) +
                $" type='number' data-type='{InputTypeEstr.Decimal}' value='{dto.Value}'" +
                //GetRequired(dto.Required) +
                GetPlaceHolder(dto.InputTip);
            //attr += " digits='true'";   //for digital only, decimal remark !!

            if (dto.Min > 0)
                attr += " min='" + dto.Min + "'";
            if (dto.Max > 0)
                attr += " max='" + dto.Max + "'";

            //html
            var css = GetCssClass("form-control xi-box", dto.ClsBox, dto.Width);
            var html = $"<input{attr} class='{css}'>";

            //add title
            if (_Str.NotEmpty(dto.Title))
                html = InputAddLayout(html, dto.Required, dto);
            return html;
        }
        public static string XiDt(XiDtDto dto)
        {
            //fids:0(date),1(hour),2(minute)
            string date = "", hour = "", min = "";
            if (_Str.NotEmpty(dto.Value))
            {
                var dt = _Date.CsToDt(dto.Value)!.Value;
                date = dt.Date.ToString();
                hour = dt.Hour.ToString();
                min = dt.Minute.ToString();
            }

            var html = string.Format($@"
<div data-fid='{0}' data-type='{InputTypeEstr.DateTime}' class='xi-box {1}' {2}>
    {3}
    {4}
    <span>:</span>
    {5}
</div>",
dto.Fid, dto.ClsBox, dto.InputAttr,
GetDateHtml("", date, "", dto.Required, dto.Edit, dto.InputTip),
GetSelectHtml("", hour, "", _Date.GetHourList(), false, dto.Edit, false, clsBox: "xi-dt-hour", fnOnChange: dto.FnOnChange, eventArgs: dto.EventArgs),
GetSelectHtml("", min, "", _Date.GetMinuteList(dto.MinuteStep), false, dto.Edit, false, clsBox: "xi-dt-min", fnOnChange: dto.FnOnChange, eventArgs: dto.EventArgs)
);

            if (_Str.NotEmpty(dto.Title))
            {
                dto.Cols ??= "2,5";
                html = InputAddLayout(html, dto.Required, dto);
            }
            return html;
        }
        public static string XiFile(XiFileDto dto)
        {
            /*
            if (_Str.IsEmpty(dto.FnOnViewFile))
                dto.FnOnViewFile = $"_me.onViewFile(\"{dto.Table}\", \"{dto.Fid}\", this)";
            dto.FnOnViewFile = GetLinkFn(dto.FnOnViewFile);
            */

            //attr, add data-table for onViewFile()
            var attr = GetInputAttr(dto.Fid, dto.Edit, dto.Required, dto.InputAttr);

            if (dto.MaxSize <= 0)
                dto.MaxSize = _Fun.Config.UploadFileMax;

            //fileType to file Ext list
            var exts = _File.TypeToExts(dto.FileType);
            var dataEdit = GetDataEdit(dto.Edit);

            //if container is label, inside element onclick will trigger when click inside !!
            //hidden input text for validate msg placement
            //data-max/exts for checking so put in input file, others for input hide !!
            //button open/delete will be handled by status, but link(view) is on.
            var html = $@"
<div class='form-control xi-box xi-box-file {dto.ClsBox}'>
    <input type='file' data-max='{dto.MaxSize}' data-exts='{exts}' data-onchange='_ifile.onChangeFile' class='d-none'>
    <input{attr} data-type='{InputTypeEstr.File}' type='hidden' class='xd-valid'>

    <button type='button' class='btn btn-link' data-onclick='_ifile.onOpenFile' {dataEdit}>
        <i class='ico-open'></i>
    </button>
    <button type='button' class='btn btn-link' data-onclick='_ifile.onDeleteFile' {dataEdit}>
        <i class='ico-delete'></i>
    </button>
    <a href='#' class='btn btn-link' data-onclick='_me.onViewFile' data-args='{dto.Table},{dto.Fid}'>{dto.Value}</a>
</div>";

            //add label if need
            if (_Str.NotEmpty(dto.Title))
                html = InputAddLayout(html, dto.Required, dto);
            return html;
        }
        public static string XiHide(XiHideDto dto)
        {
            var attr = GetInputAttr(dto.Fid, "", false, dto.InputAttr);
            //if (_Str.NotEmpty(dto.BoxClass))
            //    attr += $" class='{dto.BoxClass}'";

            var html = $"<input{attr} data-type='{InputTypeEstr.Text}' type='hidden' value='{dto.Value}'>";
            return html;
        }
        public static string XiHtml(XiHtmlDto dto)
        {
            var attr = GetInputAttr(dto.Fid, dto.Edit, dto.Required, dto.InputAttr) +
                $" value='{dto.Value}'" +
                GetPlaceHolder(dto.InputTip) +
                //GetRequired(dto.Required) +
                GetMaxLength(dto.MaxLen);

            //summernote will add div below textarea, so add div outside for validate msg
            var css = GetCssClass("form-control xd-valid", dto.ClsBox, dto.Width);
            var html = $@"
<div class='xi-box'>
    <textarea{attr} data-type='{InputTypeEstr.Html}' class='{css}'></textarea>
</div>";
            if (_Str.NotEmpty(dto.Title))
                html = InputAddLayout(html, dto.Required, dto);
            return html;
        }
        public static string XiInt(XiIntDto dto)
        {
            //attr, both digits/number should be type=number for validate(digits not work !!)
            var attr = GetInputAttr(dto.Fid, dto.Edit, dto.Required, dto.InputAttr) +
                $" type='number' data-type='{InputTypeEstr.Integer}' value='{dto.Value}'" +
            //GetRequired(dto.Required) +
            GetPlaceHolder(dto.InputTip);
            attr += " digits='true'";   //for digital only, decimal remark !!

            if (dto.Min > 0)
                attr += " min='" + dto.Min + "'";
            if (dto.Max > 0)
                attr += " max='" + dto.Max + "'";

            //html
            var css = GetCssClass("form-control xi-box", dto.ClsBox, dto.Width);
            var html = $"<input{attr} class='{css}'>";

            //add title
            if (_Str.NotEmpty(dto.Title))
                html = InputAddLayout(html, dto.Required, dto);
            return html;
        }
        public static string XiLink(XiLinkDto dto)
        {
            //add class xi-unsave for not save Db !!
            var attr = GetInputAttr(dto.Fid, "", false, dto.InputAttr) +
                $" data-type='{InputTypeEstr.Link}' data-onclick='_me.onViewFile' data-args='{dto.Table},{dto.Fid}'";

            var css = GetCssClass("xi-unsave", dto.ClsBox, dto.Width);
            var html = $"<a href='#' {attr} class='{css}'>{dto.Value}</a>";

            //add title if need
            if (_Str.NotEmpty(dto.Title))
                html = InputAddLayout(html, false, dto);
            return html;
        }
        public static string XiRadio(XiRadioDto dto)
        {
            //box & ext class
            //var boxClass = "xi-box"; 
            if (dto.IsHori)
                dto.ClsBox += " x-inline";
            //if (extClass != "")
            //    extClass = " " + extClass;

            //default input this
            //prop.FnOnChange = GetFnOnChange("onclick", prop, "this");

            //one radio (span for radio sign)
            var tplItem = @"
<label class='xi-check'>
	<input type='radio'{0}>{1}
	<span class='xi-rspan'></span>
</label>
";
            var list = "";
            for (var i = 0; i < dto.Rows.Count; i++)
            {
                //change empty to nbsp, or radio will be wrong !!
                var row = dto.Rows[i];
                if (row.Str == "")
                    row.Str = "&nbsp;";

                //get attr, value attr will disappear, use data-value instead !!
                var attr = GetInputAttr(dto.Fid, dto.Edit, false, dto.InputAttr) +
                    $" name='{dto.Fid}' data-value='{row.Id}' data-type='{InputTypeEstr.Radio}'" +
                    (_Str.IsEmpty(dto.FnOnClick) ? "" : $" data-onclick='{dto.FnOnClick}'") +
                    (row.Id == dto.Value ? " checked" : "");
                list += string.Format(tplItem, attr, row.Str);
            }

            //get html
            var html = $"<div class='xi-box {dto.ClsBox}'>{list}</div>";

            //add title outside
            //consider this field could in datatable(no title) !!
            if (_Str.NotEmpty(dto.Title))
                html = InputAddLayout(html, false, dto, true);
            return html;
        }
        public static string XiRead(XiReadDto dto)
        {
            var attr = GetInputAttr(dto.Fid, "", false, dto.InputAttr);
            if (_Str.NotEmpty(dto.Format))
                attr += $" data-format='{dto.Format}'";

            //xiRead 無條件加上 x-inline
            //xi-read2 表示 edit style
            var css = "x-inline" + (dto.EditStyle ? " xi-read2" : " xi-read");
            if (dto.Width > 0)
                css += $" x-w{dto.Width}";
            if (dto.ClsBox != "")
                css += " " + dto.ClsBox;
            //add class xi-unsave for not save DB, _form.js toJson() will filter out it !!
            if (!dto.SaveDb)
                css += " xi-unsave";
            var html = $"<label{attr} data-type='{InputTypeEstr.ReadOnly}' class='form-control {css}'>{dto.Value}</label>";

            if (_Str.NotEmpty(dto.Title))
                html = InputAddLayout(html, false, dto);
            return html;
        }
        public static string XiSelect(XiSelectDto dto)
        {
            var html = GetSelectHtml(dto.Fid, dto.Value, InputTypeEstr.Select, dto.Rows!,
                dto.Required, dto.Edit, dto.AddEmptyRow,
                dto.InputTip, dto.InputAttr, dto.ClsBox, dto.FnOnChange, dto.EventArgs);

            if (_Str.NotEmpty(dto.Title))
                html = InputAddLayout(html, dto.Required, dto);
            return html;
        }
        public static string XiText(XiTextDto dto)
        {
            //base attr: fid,name,readonly,ext attr
            var type = dto.IsPwd ? "password" : "text";
            var attr = GetInputAttr(dto.Fid, dto.Edit, dto.Required, dto.InputAttr) +
                $" type='{type}' value='{dto.Value}'" +
                GetPlaceHolder(dto.InputTip) +
                GetMaxLength(dto.MaxLen) +
                GetPattern(dto.Pattern);

            //get input html, xi-box for 控制 validate error 位置
            var css = GetCssClass("form-control xi-box", dto.ClsBox, dto.Width);
            var html = $"<input{attr} data-type='{InputTypeEstr.Text}' class='{css}'>";

            //add title,required,tip,cols for single form
            //consider this field could in datatable(no title) !!
            if (_Str.NotEmpty(dto.Title))
                html = InputAddLayout(html, dto.Required, dto);
            return html;
        }
        public static string XiTextarea(XiTextareaDto dto)
        {
            //attr
            var attr = GetInputAttr(dto.Fid, dto.Edit, dto.Required, dto.InputAttr) +
                $" value='{dto.Value}' rows='{dto.RowsCount}'" +
                GetPlaceHolder(dto.InputTip) +
                GetMaxLength(dto.MaxLen);

            //html
            var css = GetCssClass("form-control xi-box", dto.ClsBox, dto.Width);
            var html = $"<textarea{attr} data-type='{InputTypeEstr.Textarea}' class='{css}'>{dto.Value}</textarea>";
            if (_Str.NotEmpty(dto.Title))
                html = InputAddLayout(html, dto.Required, dto);
            return html;
        }
        public static string XgGroup(string label, bool icon)
        {
            //var text = _Str.Repeat(10, "· • ●");
            var line = _Str.Repeat(10, "· • · • ");
            var iconHtml = icon 
                ? "<i class='ico open'></i>" 
                : "";
            return $@"
<div class='x-group'>
    <span class='x-group-label'>{label}
        {iconHtml}
    </span>
    <div class='x-group-line'>{line}
    </div>
</div>
";
        }
        #endregion
    }//class
}

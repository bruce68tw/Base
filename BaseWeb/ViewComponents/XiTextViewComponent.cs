using Base.Services;
using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>
    /// text input
    /// xi-box用來顯示 validation error(相鄰位置), 大多數欄位的xi-box即為輸入欄位
    /// BoxClass會加到 xi-box上面
    /// </summary>
    public class XiTextViewComponent : ViewComponent
    {
        public HtmlString Invoke(XiTextDto dto)
        {
            /*
             string title, string fid, string value,
            string edit = "", bool inRow = false, bool required = false,  
            string type = "text", int maxLen = 0, string width = "100%",
            string labelTip = "", string inputTip = "", string extAttr = "", string extClass = "", 
            string cols = ""
             */
            //prop ??= new PropTextDto();

            //base attr: fid,name,readonly,ext attr
            var type = dto.IsPwd ? "password" : "text";
            var attr = _Helper.GetInputAttr(dto.Fid, dto.Edit, dto.Required, dto.InputAttr) +
                $" type='{type}' value='{dto.Value}' style='width:{dto.Width}'" +     //default 100%
                _Helper.GetPlaceHolder(dto.InputTip) +
                //_Helper.GetRequired(dto.Required) +
                _Helper.GetMaxLength(dto.MaxLen);

            //get input html
            var html = $"<input{attr} data-type='text' class='form-control xi-box {dto.BoxClass}'>";

            //add title,required,tip,cols for single form
            //consider this field could in datatable(no title) !!
            if (_Str.NotEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, dto.Required, dto.LabelTip, dto.InRow, dto.Cols);
            return new HtmlString(html);
        } 

    }//class
}

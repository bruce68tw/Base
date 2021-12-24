using Base.Services;
using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    //integer/decimal input(3 different: class name, input arg, digit=true !!)
    public class XiDecViewComponent : ViewComponent
    {
        public HtmlString Invoke(XiDecDto dto)
        {
            //attr, both digits/number should be type=number for validate(digits not work !!)
            var attr = _Helper.GetInputAttr(dto.Fid, dto.Edit, dto.Required, dto.InputAttr) +
                $" type='number' data-type='dec' value='{dto.Value}' style='text-align:right; width:{dto.Width}'" +
                _Helper.GetRequired(dto.Required) +
                _Helper.GetPlaceHolder(dto.InputTip);
            //attr += " digits='true'";   //for digital only, decimal remark !!

            if (dto.Min > 0)
                attr += " min='" + dto.Min + "'";
            if (dto.Max > 0)
                attr += " max='" + dto.Max + "'";

            //html
            var html = $"<input{attr} class='form-control xi-box {dto.BoxClass}'>";

            //add title
            if (!_Str.IsEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, dto.Required, dto.LabelTip, dto.InRow, dto.Cols);
            return new HtmlString(html);
        } 

    }//class
}

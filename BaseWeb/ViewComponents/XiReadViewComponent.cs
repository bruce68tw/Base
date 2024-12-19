﻿using Base.Services;
using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;

namespace BaseWeb.ViewComponents
{
    //readonly field
    public class XiReadViewComponent
    {
        public HtmlString Invoke(XiReadDto dto)
        {
            var attr = _Helper.GetInputAttr(dto.Fid, "", false, dto.InputAttr);
            if (_Str.NotEmpty(dto.Format))
                attr += $" data-format='{dto.Format}'";

            //add class xi-unsave for not save DB, _form.js toJson() will filter out it !!
            var cls = dto.BoxClass + (dto.EditStyle ? " xi-read2" : " xi-read");
            if (!dto.SaveDb)
                cls += " xi-unsave";
            //xi-read for css style
            var html = $"<label{attr} data-type='read' class='form-control {cls}'>{dto.Value}</label>";

            if (_Str.NotEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, false, dto.LabelTip, dto.InRow, dto.Cols);
            return new HtmlString(html);
        }

    }//class
}

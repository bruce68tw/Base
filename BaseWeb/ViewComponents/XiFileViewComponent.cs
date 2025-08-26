﻿using Base.Services;
using BaseWeb.Models;
using BaseWeb.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace BaseWeb.ViewComponents
{
    /// <summary>
    /// file input
    /// </summary>
    public class XiFileViewComponent : ViewComponent
    {
        /// <summary>
        /// file upload, 
        /// </summary>
        /// <returns></returns>
        public HtmlString Invoke(XiFileDto dto)
        {
            /*
            if (_Str.IsEmpty(dto.FnOnViewFile))
                dto.FnOnViewFile = $"_me.onViewFile(\"{dto.Table}\", \"{dto.Fid}\", this)";
            dto.FnOnViewFile = _Helper.GetLinkFn(dto.FnOnViewFile);
            */

            //attr, add data-table for onViewFile()
            var attr = _Helper.GetInputAttr(dto.Fid, dto.Edit, dto.Required, dto.InputAttr);

            if (dto.MaxSize <= 0)
                dto.MaxSize = _Fun.Config.UploadFileMax;

            //fileType to file Ext list
            var exts = _File.TypeToExts(dto.FileType);
            var dataEdit = _Helper.GetDataEdit(dto.Edit);

            //if container is label, inside element onclick will trigger when click inside !!
            //hidden input text for validate msg placement
            //data-max/exts for checking so put in input file, others for input hide !!
            //button open/delete will be handled by status, but link(view) is on.
            var html = $@"
<div class='form-control xi-box xi-box-file {dto.BoxClass}'>
    <input type='file' data-max='{dto.MaxSize}' data-exts='{exts}' data-onchange='_ifile.onChangeFile' class='d-none'>
    <input{attr} data-type='file' type='hidden' class='xd-valid'>

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
                html = _Helper.InputAddLayout(html, dto.Title, dto.Required, dto.LabelTip, dto.InRow, dto.Cols);

            return new HtmlString(html);
        }
        
    } //class
}

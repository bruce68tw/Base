using Base.Services;
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
        /// <param name="title"></param>
        /// <param name="fid"></param>
        /// <param name="cols"></param>
        /// <param name="fileType">I(image),E(excel),W(word)</param>
        /// <returns></returns>
        public HtmlString Invoke(XiFileDto dto)
        {
            /*
             string title, string fid, string value = "",
            bool required = false, bool inRow = false, string cols = "",
            string labelTip = "", int maxSize = 0, string fileType = "I",
            string extAttr = "", string extClass = "", string edit = "",
            string fnOnViewFile = "",
            string fnOnOpenFile = "_ifile.onOpenFile(this)",
            string fnOnDeleteFile = "_ifile.onDeleteFile(this)"
             */
            if (_Str.IsEmpty(dto.FnOnViewFile))
                dto.FnOnViewFile = $"_me.onViewFile(\"{dto.Table}\", \"{dto.Fid}\", this)";
            dto.FnOnViewFile = _Helper.GetLinkFn(dto.FnOnViewFile);

            //attr, add data-table for onViewFile()
            var attr = _Helper.GetInputAttr(dto.Fid, dto.Edit, dto.Required, dto.InputAttr);

            if (dto.MaxSize <= 0)
                dto.MaxSize = _Fun.Config.UploadFileMax;

            //fileType to file Ext list
            var exts = _File.TypeToExts(dto.FileType);
            var dataEdit = _Helper.GetDataEdit(dto.Edit);

            //if container is label, inside element onclick will trigger when click inside !!
            //data-max/exts for input file for checking, others for input hide !!
            //hidden input text for validate msg placement
            //data-max, data-exts is checking when change file, so put in input file.
            //button open/delete will be handled by status, but link(view) is not.
            var html = $@"
<div class='form-control xi-box {dto.BoxClass}' style='margin-bottom:0'>
    <input type='file' data-max='{dto.MaxSize}' data-exts='{exts}' onchange='_ifile.onChangeFile(this)' style='display:none'>
    <input{attr} data-type='file' type='hidden'>

    <button type='button' class='btn btn-link' onclick='_ifile.onOpenFile(this)' {dataEdit}>
        <i class='ico-open'></i>
    </button>
    <button type='button' class='btn btn-link' onclick='_ifile.onDeleteFile(this)' {dataEdit}>
        <i class='ico-delete'></i>
    </button>
    <a href='#' onclick='{dto.FnOnViewFile}'>{dto.Value}</a>
</div>";

            //add label if need
            if (!_Str.IsEmpty(dto.Title))
                html = _Helper.InputAddLayout(html, dto.Title, dto.Required, dto.LabelTip, dto.InRow, dto.Cols);

            return new HtmlString(html);
        }
        
    } //class
}

using Base.Services;
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
        /// file upload
        /// </summary>
        /// <param name="title"></param>
        /// <param name="fid"></param>
        /// <param name="cols"></param>
        /// <param name="fileType">I(image),E(excel),W(word)</param>
        /// <returns></returns>
        public HtmlString Invoke(string title, string fid, string value = "",
            bool required = false, bool inRow = false, string cols = "",
            string labelTip = "", int maxSize = 0, string fileType = "I",
            string extAttr = "", string extClass = "",
            string fnOnViewFile = "_me.onViewFile(this)",
            string fnOnOpenFile = "_ifile.onOpenFile(this)",
            string fnOnDeleteFile = "_ifile.onDeleteFile(this)")
        {

            var attr = _Helper.GetInputAttr(fid, true, required);
            if (maxSize <= 0)
                maxSize = _Fun.Config.UploadFileMax;
            //fileType to file Ext list
            var exts = _File.TypeToExts(fileType);
            //var attr = $" data-type='file' data-max='{maxSize}' data-exts='{exts}'";

            //hidden input text for validate msg placement
            //data-max, data-exts is checking when change file, so put in input file.
            //button open/delete will be handled by status, but link(view) is not.
            //need hidden input text for validate
            var html = $@"
<div class='xi-file {extClass}' {extAttr}>
    <label class='form-control' style='margin-bottom:0'>
        <button type='button' class='btn btn-link' onclick='{fnOnOpenFile}'>
            <i class='ico-open'></i>
        </button>
        <button type='button' class='btn btn-link' onclick='{fnOnDeleteFile}'>
            <i class='ico-delete'></i>
        </button>
        <a href='#' onclick='event.preventDefault(); {fnOnViewFile}'>{value}</a>
    </label>
    <input name='{fid}' type='file' data-max='{maxSize}' data-exts='{exts}' onchange='_ifile.onChangeFile(this)' style='display:none'>
    <input{attr} data-type='file' type='hidden'>
</div>";

            //add label if need
            if (!string.IsNullOrEmpty(title))
                html = _Helper.InputAddLayout(html, title, required, labelTip, inRow, cols);

            return new HtmlString(html);
        }
        
    } //class
}

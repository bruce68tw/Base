using Base.Enums;

namespace BaseWeb.Models
{
    public class XiFileDto : XiBaseDto
    {
        /*
        public XiFileDto()
        {
            MaxSize = 10;
            FileType = "I"; //image
            //FnOnOpenFile = "_ifile.onOpenFile(this)";
            //FnOnDeleteFile = "_ifile.onDeleteFile(this)";
        }
        */

        //for onViewFile()
        public string Table { get; set; } = "";

        public int MaxSize { get; set; } = 10;

        public UpFileTypeEnum FileType { get; set; } = UpFileTypeEnum.Image;

        //FileType=UpFileTypeEnum.Custom時, 必須設定此欄位, 逗號分隔的副檔名, 不帶點, 例如: "jpg,png,gif"
        public string FileTypeExts { get; set; } = "";

        //取消, 固定呼叫_me.onViewFile, 同時傳入table, fid
        //public string FnOnViewFile { get; set; } = "";
        //public string FnOnOpenFile { get; set; }
        //public string FnOnDeleteFile { get; set; }
    }
}
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

        //*(all),I(image),E(excel),W(word)
        public string FileType { get; set; } = "I"; //image

        //取消, 固定呼叫_me.onViewFile, 同時傳入table, fid
        //public string FnOnViewFile { get; set; } = "";
        //public string FnOnOpenFile { get; set; }
        //public string FnOnDeleteFile { get; set; }
    }
}
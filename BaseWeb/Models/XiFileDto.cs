namespace BaseWeb.Models
{
    public class XiFileDto : XiBaseDto
    {
        public XiFileDto()
        {
            MaxSize = 10;
            FileType = "I"; //image
            FnOnOpenFile = "_ifile.onOpenFile(this)";
            FnOnDeleteFile = "_ifile.onDeleteFile(this)";
        }

        public int MaxSize { get; set; }

        //I(image),E(excel),W(word)
        public string FileType { get; set; }
        public string FnOnViewFile { get; set; }
        public string FnOnOpenFile { get; set; }
        public string FnOnDeleteFile { get; set; }
    }
}
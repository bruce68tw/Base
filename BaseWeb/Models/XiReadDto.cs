namespace BaseWeb.Models
{
    public class XiReadDto
    {
        public XiReadDto()
        {
            InRow = false;
            SaveDb = false; //default not save Db !!
        }

        public string Title { get; set; }
        public string Fid { get; set; }
        public string Value { get; set; }
        public bool InRow  { get; set; }
        public string LabelTip  { get; set; }
        public string InputAttr  { get; set; }
        public string BoxClass  { get; set; }
        public string Cols { get; set; }
        public string Format { get; set; }
        public bool SaveDb { get; set; }
    }
}
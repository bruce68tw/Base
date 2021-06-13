namespace BaseWeb.Models
{
    public class XiReadDto
    {
        public XiReadDto()
        {
            InRow = false;
        }

        public string Title { get; set; }
        public string Fid { get; set; }
        public string Value { get; set; }
        public bool InRow  { get; set; }
        public string LabelTip  { get; set; }
        public string ExtAttr  { get; set; }
        public string ExtClass  { get; set; }
        public string Cols { get; set; }
        public string Format { get; set; }
    }
}
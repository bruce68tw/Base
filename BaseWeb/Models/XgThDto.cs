namespace BaseWeb.Models
{
    public class XgThDto
    {
        public XgThDto()
        {
            Title = "";
            Tip = "";
            Required = false;
            ExtClass = "";
        }

        public string Title { get; set; }
        public string Tip { get; set; }
        public bool Required { get; set; }
        public string ExtClass { get; set; }
    }
}
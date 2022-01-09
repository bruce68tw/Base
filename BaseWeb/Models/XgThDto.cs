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
            HideRwd = false;
            MinWidth = 0;
        }

        public string Title { get; set; }
        public string Tip { get; set; }
        public bool Required { get; set; }
        public string ExtClass { get; set; }

        //min width, 0 for not set
        public int MinWidth { get; set; }

        //hide for RWD phone
        public bool HideRwd { get; set; }
    }
}
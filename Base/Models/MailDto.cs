using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// smtp email
    /// </summary>
    public class MailDto
    {
        public string Subject { get; set; }

        //email body
        public string Body { get; set; }

        public List<string> ToUsers { get; set; }

        public List<string> CcUsers { get; set; }

        //attach files
        public List<string> Files { get; set; }

        //image id list
        public List<string> ImageIds { get; set; }

        //image path list
        public List<string> ImagePaths { get; set; }
    }
}

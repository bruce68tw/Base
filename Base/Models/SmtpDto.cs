
namespace Base.Models
{
    /// <summary>
    /// smtp email
    /// </summary>
    public class SmtpDto
    {
        public string FromEmail { get; set; }

        //display sender name when get email
        public string FromName { get; set; }    

        public string Host { get; set; }
        public int Port { get; set; }

        //sender account
        public string Id { get; set; }  

        public string Pwd { get; set; }
        public bool Ssl = true;
    }
}

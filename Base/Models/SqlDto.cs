
namespace Base.Models
{
    //sql string each block
    public class SqlDto
    {
        //sql statement without "Select" key word
        public string Select = "";

        //column list
        public string[] Columns = null;     

        public string From = "";
        public string Where = "";
        public string Group = "";
        public string Order = "";
    }
}
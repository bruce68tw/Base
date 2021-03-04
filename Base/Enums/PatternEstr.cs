
namespace Base.Enums
{
    /// <summary>
    /// input pattern
    /// </summary>
    public class PatternEstr
    {
        //alphabetic
        //public const string AlphaNum = "[A-Z0-9a-z]";

        //numeric
        //public const string Number = "[\\d]";

        //alphabetic, underline(c# need escape)
        public const string Word = @"^[\w]+$";

        //numeric, space
        public const string Phone = @"^[\d\s]+$";

        //Word, chinese, space, ".", "_"
        public const string Normal = @"^[\u4e00-\u9fa5\w\s._]+$";
        
        //html
        //public const string Html = "textarea";

    }
}

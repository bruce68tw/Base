
namespace Base.Enums
{
    /// <summary>
    /// input pattern
    /// </summary>
    public class InputPatternEstr
    {
        public const string None = "none";      //中文
        public const string EngNum = "[A-Za-z0-9]+";    //英數
        public const string EngNumExt = "[A-Za-z0-9\\(\\)\\-\\/ ]+"; //英數()-/(空白), 此為default
    }
}

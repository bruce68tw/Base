namespace Base.Models
{
    //LLM 問題
    public class LlmAskDto
    {
        //system role
        public string System { get; set; } = "";
        //user role
        public string User { get; set; } = "";
    }
}

namespace Base.Models
{
    /// <summary>
    /// for send server side result to front end
    /// </summary>
    public class ResultDto
    {
        /// <summary>
        /// error code if any
        /// </summary>
        public string Code = "";

        /// <summary>
        /// result value
        /// </summary>
        public string Value = "";

        /// <summary>
        /// error msg if any
        /// </summary>
        public string ErrorMsg = "";

    }
}
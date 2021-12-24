
namespace Base.Models
{
    /// <summary>
    /// import result dto, namig for file sorting
    /// </summary>
    public class ResultImportDto
    {
        /// <summary>
        /// ok rows count
        /// </summary>
        public int OkCount;

        /// <summary>
        /// fail rows count
        /// </summary>
        public int FailCount;

        /// <summary>
        /// total rows count
        /// </summary>
        public int TotalCount;

        /// <summary>
        /// error msg if any (necessary field for resultXXX dto)
        /// </summary>
        public string ErrorMsg = "";

    }
}
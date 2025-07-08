
using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// for send server side result to front end
    /// </summary>
    public class ResultDto
    {
        /// <summary>
        /// result value
        /// </summary>
        public string Value = "";

        /// <summary>
        /// error code if any
        /// </summary>
        public string Code = "";

        /// <summary>
        /// error msg if any (necessary field for resultXXX dto)
        /// 避開資料表欄位, 前面加底線
        /// </summary>
        public string _ErrorMsg = "";

        /// <summary>
        /// validation error list
        /// </summary>
        public List<ErrorRowDto>? ErrorRows = null;

        /// <summary>
        /// error js _BR fid
        /// </summary>
        //public string ErrorBrFid = "";

    }
}
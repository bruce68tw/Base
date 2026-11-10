using Base.Models;
using System;
using System.DirectoryServices;

namespace Base.Services
{
    /// <summary>
    /// 後端操作結果的錯誤判斷。
    /// </summary>
    public class _Result
    {
        /// <summary>
        /// _ErrorMsg 非空或 ErrorRows 有項目時回傳 true。
        /// </summary>
        /// <param name="result">要判斷操作是否成功的結果物件</param>
        public static bool HasError(ResultDto result)
        {
            return _Str.NotEmpty(result._ErrorMsg) 
                ? true 
                : (result.ErrorRows != null && result.ErrorRows.Count > 0);
        }

    }//class
}

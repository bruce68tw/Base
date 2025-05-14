using Base.Models;
using System;
using System.DirectoryServices;

namespace Base.Services
{
    //後端執行結果
    public class _Result
    {
        /// <summary>
        /// 檢查是否有錯誤
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool HasError(ResultDto result)
        {
            return _Str.NotEmpty(result.ErrorMsg) 
                ? true 
                : (result.ErrorRows != null && result.ErrorRows.Count > 0);
        }

    }//class
}

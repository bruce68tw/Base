
using System.Collections.Generic;

namespace Base.Models
{
    /// <summary>
    /// 後端操作結果；_Result.HasError 依錯誤訊息與欄位錯誤判斷失敗。
    /// </summary>
    public class ResultDto
    {
        /// <summary>
        /// 操作結果值，例如新增後的主鍵。
        /// </summary>
        public string Value = "";
        /// <summary>
        /// 由操作端指定的結果或錯誤代碼。
        /// </summary>
        public string Code = "";
        /// <summary>
        /// 錯誤訊息，空字串表示未設定。
        /// </summary>
        public string _ErrorMsg = "";
        /// <summary>
        /// 欄位或資料列驗證錯誤清單，預設 null。
        /// </summary>
        public List<ErrorRowDto>? ErrorRows = null;

        //public string ErrorBrFid = "";

    }
}

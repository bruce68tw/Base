using System.ComponentModel.DataAnnotations;

namespace BaseApi.Attributes
{
    /// <summary>
    /// 驗證字串長度上限，並在錯誤訊息中顯示設定的上限。
    /// </summary>
    public class XgStrLenAttribute : StringLengthAttribute
    {
        /// <summary>
        /// 以指定的最大長度初始化驗證規則和錯誤訊息。
        /// </summary>
        /// <param name="maxLen">允許的最大字串長度。</param>
        public XgStrLenAttribute(int maxLen) : base(maxLen)
        {
            //ErrorMessage = string.Format(_Locale.GetBaseRes()!.StrLen, maxLen);
            ErrorMessage = string.Format("max length={0}", maxLen);
        }

    } //class
}

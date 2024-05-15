using System.ComponentModel.DataAnnotations;

namespace BaseApi.Attributes
{
    public class XgStrLenAttribute : StringLengthAttribute
    {
        public XgStrLenAttribute(int maxLen) : base(maxLen)
        {
            //ErrorMessage = string.Format(_Locale.GetBaseRes()!.StrLen, maxLen);
            ErrorMessage = string.Format("max length={0}", maxLen);
        }

    } //class
}

using Base.Services;
using BaseWeb.Services;
using System.ComponentModel.DataAnnotations;

namespace BaseWeb.Attributes
{
    public class XgStrLenAttribute : StringLengthAttribute
    {
        public XgStrLenAttribute(int maxLen) : base(maxLen)
        {
            ErrorMessage = string.Format(_Locale.GetBaseRes().StrLen, maxLen);
        }

    } //class
}

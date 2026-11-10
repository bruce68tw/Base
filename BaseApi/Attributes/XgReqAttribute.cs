using Base.Services;
using System.ComponentModel.DataAnnotations;

namespace BaseApi.Attributes
{
    /// <summary>
    /// 沿用 RequiredAttribute 驗證必填值，並在驗證時依現有錯誤訊息設定調整成員名稱提示。
    /// </summary>
    public class XgReqAttribute : RequiredAttribute
    {
        /*
        public XgReqAttribute()
        {
            ErrorMessage = _Fun.GetBaseRes().Required;
        }
        */

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            //_Locale.GetBaseRes()!.Required
            var result = base.IsValid(value, validationContext);
            if (_Str.NotEmpty(ErrorMessage))
                ErrorMessage = validationContext.MemberName + " Required";
            return result!;
        }

    } //class
}

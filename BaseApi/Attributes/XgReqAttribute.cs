using Base.Services;
using System.ComponentModel.DataAnnotations;

namespace BaseApi.Attributes
{
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

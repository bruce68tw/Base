using Base.Services;
using BaseWeb.Services;
using System.ComponentModel.DataAnnotations;

namespace BaseWeb.Attributes
{
    public class XgReqAttribute : RequiredAttribute
    {
        /*
        public XgReqAttribute()
        {
            ErrorMessage = _Fun.GetBaseRes().Required;
        }
        */

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var result = base.IsValid(value, validationContext);
            if (!_Str.IsEmpty(ErrorMessage))
                ErrorMessage = validationContext.MemberName + _Locale.GetBaseRes().Required;

            return result;
        }

    } //class
}

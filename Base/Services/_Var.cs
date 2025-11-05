using DocumentFormat.OpenXml.Office2013.PowerPoint.Roaming;

namespace Base.Services
{
    public class _Var
    {
        public static bool IsEmpty(object? data)
        {
            return (data == null || data.ToString() == "");
        }

        public static bool NotEmpty(object? data)
        {
            return !_Var.IsEmpty(data);
        }

        public static bool ToBool(object? data)
        {
            if (data == null) return false;
            var value = data.ToString()!;
            return (value == "1" || value.ToLower() == "true");
                ;
        }

    }
}

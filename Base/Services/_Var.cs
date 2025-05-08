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
    }
}

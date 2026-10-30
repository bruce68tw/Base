namespace Base.Services
{
    /// <summary>
    /// 物件字串表示的空值與布林判斷。
    /// </summary>
    public class _Var
    {
        /// <summary>
        /// 物件為 null 或字串表示為空時回傳 true。
        /// </summary>
        public static bool IsEmpty(object? data)
        {
            return (data == null || data.ToString() == "");
        }

        /// <summary>
        /// 物件非 null 且字串表示非空時回傳 true。
        /// </summary>
        public static bool NotEmpty(object? data)
        {
            return !_Var.IsEmpty(data);
        }

        /// <summary>
        /// 僅 1 或不分大小寫的 true 視為 true；null 視為 false。
        /// </summary>
        public static bool ToBool(object? data)
        {
            if (data == null) return false;
            var value = data.ToString()!;
            return (value == "1" || value.ToLower() == "true");
                ;
        }

    }
}

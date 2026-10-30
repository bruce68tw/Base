namespace Base.Enums
{
    /// <summary>
    /// 字元驗證使用的正規表示式常數。
    /// </summary>
    public class PatternEstr
    {
        /// <summary>
        /// 一個以上的正規表示式單字字元，包含 Unicode 字母、數字與底線等。
        /// </summary>
        public const string Word = @"^[\w]+$";
        /// <summary>
        /// 只允許數字與空白，並非完整電話號碼格式驗證。
        /// </summary>
        public const string Phone = @"^[\d\s]+$";
        /// <summary>
        /// 允許指定中文字範圍、單字字元、空白、句點及底線。
        /// </summary>
        public const string Normal = @"^[\u4e00-\u9fa5\w\s._]+$";
    }
}

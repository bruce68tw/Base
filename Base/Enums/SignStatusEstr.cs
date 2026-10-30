namespace Base.Enums
{
    /// <summary>
    /// 個別簽核紀錄的狀態代碼。
    /// </summary>
    public class SignStatusEstr
    {
        /// <summary>
        /// 尚未簽核。
        /// </summary>
        public const string None = "0";
        /// <summary>
        /// 簽核中。
        /// </summary>
        public const string Work = "1";
        /// <summary>
        /// 同意。
        /// </summary>
        public const string Agree = "Y";
        /// <summary>
        /// 退回。
        /// </summary>
        public const string Back = "N";
    }
}

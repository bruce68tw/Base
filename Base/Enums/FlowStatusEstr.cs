namespace Base.Enums
{
    /// <summary>
    /// 簽核流程整體的狀態代碼。
    /// </summary>
    public class FlowStatusEstr
    {
        /// <summary>
        /// 簽核進行中。
        /// </summary>
        public const string Work = "1";
        /// <summary>
        /// 流程同意完成。
        /// </summary>
        public const string Agree = "Y";
        /// <summary>
        /// 流程退回。
        /// </summary>
        public const string Back = "N";
    }
}

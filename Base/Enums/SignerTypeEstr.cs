namespace Base.Enums
{
    /// <summary>
    /// 簽核對象的指定方式代碼。
    /// </summary>
    public static class SignerTypeEstr
    {
        /// <summary>
        /// 使用者類型；目前 _XgFlow 以流程擁有者取得使用者。
        /// </summary>
        public const string User = "U";
        /// <summary>
        /// 以指定資料欄位取得使用者。
        /// </summary>
        public const string Fid = "F";
        /// <summary>
        /// 流程擁有者的主管。
        /// </summary>
        public const string UserMgr = "UM";
        /// <summary>
        /// 指定部門的主管。
        /// </summary>
        public const string DeptMgr = "DM";
        /// <summary>
        /// 指定角色的使用者。
        /// </summary>
        public const string Role = "R";
        /// <summary>
        /// 指定部門角色的使用者。
        /// </summary>
        public const string DeptRole = "DR";
    }
}

namespace Base.Enums
{
    //match to Code.Type="xfSignerType"
    public static class SignerTypeEstr
    {
        public const string User = "U";         //指定用戶(帳號)
        public const string Fid = "F";          //指定欄位
        public const string UserMgr = "UM";     //用戶主管
        public const string DeptMgr = "DM";     //部門主管
        public const string Role = "R";         //指定角色Id(多筆)
        public const string DeptRole = "DR";    //指定部門角色Id
    }
}
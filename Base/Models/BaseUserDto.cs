namespace Base.Models
{
    /// <summary>
    /// basic user profile data for base class
    /// </summary>
    public class BaseUserDto
    {
        //login or not
        //public bool IsLogin;
        public bool HasPwd = false;

        //locale code
        public string Locale = "";

        //user id/name
        public string UserId = "";
        public string UserName = "";

        //dept id/name
        public string DeptId = "";
        public string DeptName = "";

        //user type, PG define
        public string UserType = "";

        /// <summary>
        /// 額外欄位值, 呼叫 _Login.cs LoginByVoA() 傳入, 後續透過 _Fun.GetBaseUser().ExtCol 讀取
        /// 超過2個欄位則使用合併
        /// </summary>
        public string ExtCol = "";
        public string ExtCol2 = "";
        public string ExtCol3 = "";

        //diffence hours to GMT for Db saving
        //public double HourDiff = 0;

        /// <summary>
        /// authed program list, format=progId:CRUDPEV,...
        /// </summary>
        public string ProgAuthStrs = "";

    }//class
}

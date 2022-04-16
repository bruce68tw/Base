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

        //diffence hours to GMT for Db saving
        //public double HourDiff = 0;

        //program list, ex:User:CRUDPEV,Dept:CRUD...
        public string ProgAuthStrs = "";

    }//class
}

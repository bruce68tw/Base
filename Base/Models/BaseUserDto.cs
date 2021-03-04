namespace Base.Models
{
    /// <summary>
    /// basic user profile data for base class
    /// </summary>
    public class BaseUserDto
    {
        //login or not
        public bool IsLogin;

        //user id/name
        public string UserId;
        public string UserName;

        //dept id/name
        public string DeptId;
        public string DeptName;

        //locale code
        public string Locale;
        //public string FrontDtFormat;

        //diffence hours to GMT for Db saving
        //public double HourDiff = 0;

        //program list, ex:User:CRUDPEV,Dept:CRUD...
        public string ProgAuthStrs;

    }//class
}

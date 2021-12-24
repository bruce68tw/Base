using Base.Models;

namespace Base.Services
{
    public class BaseUserService : IBaseUserService
    {
        private BaseUserDto _baseUser;

        //constructor
        public BaseUserService()
        {
            _baseUser = new BaseUserDto()
            {
                Locale = _Fun.Config.Locale,
                UserId = "System",
                UserName = "System",
                //DeptId = "D01",
                //DeptName = "D01 name",
                //HourDiff = 0,
            };
        }

        //get base user info
        public BaseUserDto GetData()
        {
            return _baseUser;
        }
    }
}

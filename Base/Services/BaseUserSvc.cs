using Base.Interfaces;
using Base.Models;

namespace Base.Services
{
    public class BaseUserSvc : IBaseUserS
    {
        //private BaseUserDto _baseUser;

        /*
        //constructor
        public BaseUserService()
        {
            _baseUser = new BaseUserDto()
            {
                Locale = _Fun.Config.Locale,
                //UserId = "System",
                //UserName = "System",
                //HourDiff = 0,
            };
        }
        */

        //get base user info
        public BaseUserDto GetData()
        {
            return new BaseUserDto()
            {
                //Locale = _Fun.Config.Locale,
                //UserId = "System",
                //UserName = "System",
                //HourDiff = 0,
            };
        }
    }
}

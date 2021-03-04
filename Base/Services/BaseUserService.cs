using Base.Models;

namespace Base.Services
{
    public class BaseUserService : IBaseUserService
    {
        private BaseUserDto _baseU;

        //constructor
        public BaseUserService()
        {
            _baseU = new BaseUserDto()
            {
                UserId = "U01",
                UserName = "U01 name",
                DeptId = "D01",
                DeptName = "D01 name",
                Locale = _Fun.Config.DefaultLocale,
                //FrontDtFormat = _Fun.Config.FrontDtFormat,
                //HourDiff = 0,
            };
        }

        //get base user info
        public BaseUserDto GetData()
        {
            return _baseU;
        }
    }
}

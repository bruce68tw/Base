using Base.Interfaces;
using Base.Models;

namespace Base.Services
{
    /// <summary>
    /// IBaseUserSvc 的空白使用者實作，不處理登入或請求身分。
    /// </summary>
    public class BaseUserSvc : IBaseUserSvc
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

        /// <summary>
        /// 每次建立新的空白 BaseUserDto。
        /// </summary>
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

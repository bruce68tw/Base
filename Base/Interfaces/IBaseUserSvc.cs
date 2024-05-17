using Base.Models;

namespace Base.Interfaces
{
    public interface IBaseUserSvc
    {
        //get base info dto
        BaseUserDto GetData();
    }
}

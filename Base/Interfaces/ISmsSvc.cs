using Base.Models;
using System.Threading.Tasks;

namespace Base.Interfaces
{
    //簡訊服務介面
    public interface ISmsSvc
    {
        Task<bool> SendA(string phone, string msg);

    }
}

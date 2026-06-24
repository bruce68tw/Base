using Base.Models;
using System.Threading.Tasks;

namespace Base.Interfaces
{
    //簡訊服務介面
    public interface ILLMSvc
    {
        Task<string> AskA(string prompt, string question);

    }
}

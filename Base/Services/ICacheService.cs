using Base.Models;
using System.Threading.Tasks;

namespace Base.Services
{
    public interface ICacheService
    {

        string GetStr(string key);

        bool SetStr(string key, string value);

        bool DeleteKey(string key);

        //Task FlushDbA();
    }
}

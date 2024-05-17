using System.Threading.Tasks;

namespace Base.Interfaces
{
    public interface ICacheSvc
    {

        string? GetStr(string userId, string key);

        bool SetStr(string userId, string key, string value);

        bool DeleteKey(string userId, string key);

        Task ResetDbA();
    }
}

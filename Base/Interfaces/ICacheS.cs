using System.Threading.Tasks;

namespace Base.Interfaces
{
    public interface ICacheS
    {

        string? GetStr(string userId, string key);

        bool SetStr(string userId, string key, string value);

        bool DeleteKey(string userId, string key);

        Task ResetDbA();
    }
}

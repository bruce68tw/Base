using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    public class XgUserSvc
    {
        //不分頁
        public async Task<JArray?> FindUsersA(string account, string name, Db? db = null)
        {
            if (!string.IsNullOrEmpty(account)) account += '%';
            if (!string.IsNullOrEmpty(name)) name += '%';

            var sql = @"
select Id, Account, Name
from dbo.XpUser
where (@Account is null or Account like @Account)
and (@Name is null or Name like @Name)
order by Account
";
            return await _Db.GetRowsA(sql, ["Account", account, "Name", name], db);
        }

    } //class
}
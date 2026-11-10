using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    /// <summary>
    /// 依帳號與姓名的前綴查詢 XpUser，回傳符合的使用者資料。
    /// </summary>
    public class XgUserSvc
    {
        //不分頁
        /// <summary>
        /// 依帳號與姓名的前綴查詢使用者。傳入 db 時沿用該資料庫物件。
        /// </summary>
        /// <param name="account">要比對的帳號前綴。</param>
        /// <param name="name">要比對的姓名前綴。</param>
        /// <param name="db">可沿用的資料庫物件，省略時由共用查詢開啟。</param>
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
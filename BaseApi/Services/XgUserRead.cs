using Base.Enums;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    /// <summary>
    /// 透過共用 CRUD 服務查詢使用者列表，支援帳號與姓名條件。
    /// </summary>
    public class XgUserRead
    {
        private readonly ReadDto dto = new()
        {
            Items = [
                new() { Fid = "Account", Op = QitemOpEstr.Like },
                new() { Fid = "Name", Op = QitemOpEstr.Like },
            ],
        };

        /// <summary>
        /// 依控制器名稱及分頁查詢輸入，讀取使用者的分頁結果。
        /// </summary>
        /// <param name="ctrl">交由共用 CRUD 查詢使用的控制器名稱。</param>
        /// <param name="dt">包含查詢條件及分頁資訊的輸入。</param>
        public async Task<JObject?> GetPageA(string ctrl, DtDto dt)
        {
            return await new CrudReadSvc().GetPageA(dto, dt, ctrl);
        }

    } //class
}
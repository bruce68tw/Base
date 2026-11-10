using Base.Enums;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    /// <summary>
    /// 查詢 XpProg 的系統作業定義並回傳分頁結果。
    /// </summary>
    public class XgProgRead
    {
        private readonly ReadDto dto = new()
        {
            ReadSql = @"
select * from dbo.XpProg
order by Sort
",
            Items = [
                new() { Fid = "Code", Op = QitemOpEstr.Like },
                new() { Fid = "Name", Op = QitemOpEstr.Like },
            ],
        };

        /// <summary>
        /// 依控制器名稱及分頁查詢輸入，讀取系統作業的分頁結果。
        /// </summary>
        /// <param name="ctrl">交由共用 CRUD 查詢使用的控制器名稱。</param>
        /// <param name="dt">包含查詢條件及分頁資訊的輸入。</param>
        public async Task<JObject?> GetPageA(string ctrl, DtDto dt)
        {
            return await new CrudReadSvc().GetPageA(dto, dt, ctrl);
        }

    } //class
}
using Base.Enums;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    /// <summary>
    /// 依查詢條件讀取 XpTranLog 的資料異動紀錄並回傳分頁結果。
    /// </summary>
    public class XgTranLogRead
    {
        private readonly ReadDto dto = new()
        {
            ReadSql = @"
select *
from dbo.XpTranLog
order by Sn desc
",
            Items = new QitemDto[] {
                new() { Fid = "TableName", Op = QitemOpEstr.Like },
                new() { Fid = "ColName", Op = QitemOpEstr.Like },
                new() { Fid = "RowId" },
            },
        };

        /// <summary>
        /// 依控制器名稱及分頁查詢輸入，讀取資料異動紀錄的分頁結果。
        /// </summary>
        /// <param name="ctrl">交由共用 CRUD 查詢使用的控制器名稱。</param>
        /// <param name="dt">包含查詢條件及分頁資訊的輸入。</param>
        public async Task<JObject?> GetPageA(string ctrl, DtDto dt)
        {
            return await new CrudReadSvc().GetPageA(dto, dt, ctrl);
        }

    } //class
}
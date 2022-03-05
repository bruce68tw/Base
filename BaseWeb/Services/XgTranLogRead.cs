using Base.Enums;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseWeb.Services
{
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
                new() { Fid = "TableName", Op = ItemOpEstr.Like },
                new() { Fid = "ColName", Op = ItemOpEstr.Like },
                new() { Fid = "RowId" },
            },
        };

        public async Task<JObject> GetPageAsync(string ctrl, DtDto dt)
        {
            return await new CrudRead().GetPageAsync(dto, dt, ctrl);
        }

    } //class
}
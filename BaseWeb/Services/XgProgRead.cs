using Base.Enums;
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseWeb.Services
{
    public class XgProgRead
    {
        private readonly ReadDto dto = new()
        {
            ReadSql = @"
select * from dbo.XpProg
order by Sort
",
            Items = new QitemDto[] {
                new() { Fid = "Code", Op = ItemOpEstr.Like },
                new() { Fid = "Name", Op = ItemOpEstr.Like },
            },
        };

        public async Task<JObject> GetPageAsync(string ctrl, DtDto dt)
        {
            return await new CrudRead().GetPageAsync(dto, dt, ctrl);
        }

    } //class
}
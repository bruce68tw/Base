using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseWeb.Services
{
    public class XgEasyRptRead
    {
        private readonly ReadDto dto = new()
        {
            ReadSql = @"
select * from dbo.XpEasyRpt
order by Id
",
            Items = new QitemDto[] {
                new() { Fid = "Name" },
            },
        };

        public async Task<JObject> GetPageAsync(string ctrl, DtDto dt)
        {
            return await new CrudRead().GetPageAsync(dto, dt, ctrl);
        }

    } //class
}
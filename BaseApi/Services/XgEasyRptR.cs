using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseApi.Services
{
    public class XgEasyRptR
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

        public async Task<JObject?> GetPageA(string ctrl, DtDto dt)
        {
            return await new CrudReadSvc().GetPageA(dto, dt, ctrl);
        }

    } //class
}
using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;

namespace BaseFlow.Services
{
    public class XgFlowR
    {
        private readonly ReadDto dto = new()
        {
            ReadSql = @"
select *
from dbo.XpFlow
order by Id
",
        };

        public async Task<JObject?> GetPageA(string ctrl, DtDto dt)
        {
            return await new CrudReadSvc().GetPageA(dto, dt, ctrl);
        }

    } //class
}

using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BaseWeb.Services
{
    public class XgFlowRead
    {
        private ReadDto dto = new()
        {
            ReadSql = @"
select *
from dbo.XpFlow
order by Id
",
        };

        public async Task<JObject> GetPageAsync(string ctrl, DtDto dt)
        {
            return await new CrudRead().GetPageAsync(dto, dt, ctrl);
        }

    } //class
}

using Base.Models;
using Base.Services;
using Newtonsoft.Json.Linq;

namespace BaseWeb.Services
{
    public class XpFlowRead
    {
        private ReadDto dto = new ReadDto()
        {
            ReadSql = @"
select *
from dbo.XpFlow
order by Id
",
        };

        public JObject GetPage(string ctrl, DtDto dt)
        {
            return new CrudRead().GetPage(ctrl, dto, dt);
        }

    } //class
}
